using Ecommerce.ProductCatalog.Model;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ecommerce.ProductCatalog
{
    public class ServiceFabricProductRepository : IProductRepository
    {
        private readonly IReliableStateManager _stateManager;

        public ServiceFabricProductRepository(IReliableStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public async Task AddProduct(Product product)
        {
            var products = await _stateManager.GetOrAddAsync<IReliableDictionary<Guid, Product>>("products");
            using (var txn = _stateManager.CreateTransaction())
            {
                await products.AddOrUpdateAsync(txn, product.Id, product, (id, value) => product);
                await txn.CommitAsync();
            }
        }

        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            var products = await _stateManager.GetOrAddAsync<IReliableDictionary<Guid, Product>>("products");
            var result = new List<Product>();
            using (var txn = _stateManager.CreateTransaction())
            {
                var allProducts = await products.CreateEnumerableAsync(txn, EnumerationMode.Unordered);
                using (var enumerator = allProducts.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var current = enumerator.Current;
                        result.Add(current.Value);
                    }
                }
            }
            return result;
        }

        public async Task<Product> GetProduct(Guid productId)
        {
            Product result = null;
            var products = await _stateManager.GetOrAddAsync<IReliableDictionary<Guid, Product>>("products");
            using (var txn = _stateManager.CreateTransaction())
            {
                if(await products.ContainsKeyAsync(txn, productId))
                    result = (await products.TryGetValueAsync(txn, productId)).Value;

                await txn.CommitAsync();
            }
            return result;
        }
    }
}
