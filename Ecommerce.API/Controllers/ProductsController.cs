using Ecommerce.API.Model;
using Ecommerce.ProductCatalog.Model;
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
    public class ProductsController : ControllerBase
    {
        private IProductCatalogService _service;

        public ProductsController()
        {
            var proxyFactory = new ServiceProxyFactory(f => new FabricTransportServiceRemotingClientFactory());
            _service = proxyFactory.CreateServiceProxy<IProductCatalogService>(
                new Uri("fabric:/Ecommerce/Ecommerce.ProductCatalog"),
                new ServicePartitionKey(0));
        }

        [HttpGet]
        public async Task<IEnumerable<ApiProduct>> GetAsync()
        {
            var allProducts = await _service.GetAllProductsAsync();
            return allProducts.Select(p => new ApiProduct { 
                Id = p.Id,
                Description = p.Description,
                Name = p.Name,
                Price = p.Price,
                IsAvailable = p.Availability > 0
            });
        }

        [HttpPost]
        public async Task PostAsync([FromBody] ApiProduct product)
        {
            var newProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Availability = 100
            };
            await _service.AddProductAsync(newProduct);
        }
    }
}
