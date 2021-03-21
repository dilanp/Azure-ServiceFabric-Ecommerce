using System;

namespace Ecommerce.API.Model
{
    public class ApiBasketAddRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
