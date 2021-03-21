using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.CheckoutService.Model;
using Ecommerce.ProductCatalog.Model;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using UserActor.Interfaces;

namespace Ecommerce.CheckoutService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class CheckoutService : StatefulService, ICheckoutService
    {
        public CheckoutService(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<CheckoutSummary> CheckoutAsync(string userId)
        {
            var result = new CheckoutSummary();
            result.Date = DateTime.UtcNow;
            result.Products = new List<CheckoutProduct>();

            //Get the basket for this user
            var userActor = GetUserActor(userId);
            var basket = await userActor.GetBasket();
            //Get catalog client
            var catalogService = GetProductCatalogService();
            //Construct checkout product items by calling to the catalog
            foreach (var basketItem in basket)
            {
                var product = await catalogService.GetProduct(basketItem.ProductId);
                result.Products.Add(new CheckoutProduct
                {
                    Product = product,
                    Price = product.Price,
                    Quantity = basketItem.Quantity
                });
            }

            await AddToHistoryAsync(result);
            return result;
        }

        public async Task<CheckoutSummary[]> GetOrderHistoryAsync(string userId)
        {
            var result = new List<CheckoutSummary>();
            var history = await StateManager.GetOrAddAsync<IReliableDictionary<DateTime, CheckoutSummary>>("history");
            using (var txn = StateManager.CreateTransaction())
            {
                var allProducts = await history.CreateEnumerableAsync(txn, EnumerationMode.Unordered);
                using (var enumerator = allProducts.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(CancellationToken.None))
                    {
                        result.Add(enumerator.Current.Value);
                    }
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new []
            {
                new ServiceReplicaListener(context => new FabricTransportServiceRemotingListener(context, this))
            };
        }

        private IUserActor GetUserActor(string userId)
        {
            return ActorProxy.Create<IUserActor>(
                new ActorId(userId),
                new Uri("fabric:/Ecommerce/UserActorService"));
        }

        private IProductCatalogService GetProductCatalogService()
        {
            var proxyFactory = new ServiceProxyFactory(f => new FabricTransportServiceRemotingClientFactory());
            return proxyFactory.CreateServiceProxy<IProductCatalogService>(
                new Uri("fabric:/Ecommerce/Ecommerce.ProductCatalog"),
                new ServicePartitionKey(0));
        }

        private async Task AddToHistoryAsync(CheckoutSummary checkout)
        {
            var history = await StateManager
                .GetOrAddAsync<IReliableDictionary<DateTime, CheckoutSummary>>("history");
            using (var txn = StateManager.CreateTransaction())
            {
                await history.AddAsync(txn, checkout.Date, checkout);
                await txn.CommitAsync();
            }
        } 

    }
}
