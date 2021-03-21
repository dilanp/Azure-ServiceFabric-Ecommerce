using Ecommerce.API.Model;
using Ecommerce.CheckoutService.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private static readonly Random rnd = new Random(DateTime.UtcNow.Second);

        [Route("{userId}")]
        public async Task<ApiCheckoutSummary> CheckoutSummary(string userId)
        {
            var summary = await GetCheckoutService().CheckoutAsync(userId);
            return ToApiCheckoutSummary(summary);
        }

        [Route("history/{userId}")]
        public async Task<IEnumerable<ApiCheckoutSummary>> GetHistoryAsync(string userId)
        {
            var history = await GetCheckoutService().GetOrderHistoryAsync(userId);
            return history.Select(ToApiCheckoutSummary);
        }

        private ApiCheckoutSummary ToApiCheckoutSummary(CheckoutSummary model)
        {
            return new ApiCheckoutSummary
            {
                Products = model.Products.Select(p => new ApiCheckoutProduct
                {
                    ProductId = p.Product.Id,
                    ProductName = p.Product.Name,
                    Price = p.Price,
                    Quantity = p.Quantity
                }).ToList(),
                Date = model.Date,
                TotalPrice = model.TotalPrice
            };
        }

        private ICheckoutService GetCheckoutService()
        {
            long key = LongRandom();
            var proxyFacotory = new ServiceProxyFactory(c => new FabricTransportServiceRemotingClientFactory());
            return proxyFacotory.CreateServiceProxy<ICheckoutService>(
                new Uri("fabric:/Ecommerce/Ecommerce.CheckoutService"),
                new ServicePartitionKey(key));
        }

        private long LongRandom()
        {
            var buf = new byte[8];
            rnd.NextBytes(buf);
            return BitConverter.ToInt64(buf, 0);
        }
    }
}
