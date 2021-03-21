using Microsoft.ServiceFabric.Services.Remoting;
using System.Threading.Tasks;

namespace Ecommerce.CheckoutService.Model
{
    public interface ICheckoutService : IService
    {
        Task<CheckoutSummary> CheckoutAsync(string userId);
        Task<CheckoutSummary[]> GetOrderHistoryAsync(string userId);
    }
}
