using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce.ProductCatalog.Model
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllProducts();
        Task AddProduct(Product product);
        Task<Product> GetProduct(Guid productId);
    }
}
