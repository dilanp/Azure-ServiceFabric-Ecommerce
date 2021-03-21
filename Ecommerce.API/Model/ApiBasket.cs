﻿using Newtonsoft.Json;

namespace Ecommerce.API.Model
{
    public class ApiBasket
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }
        [JsonProperty("items")]
        public ApiBasketItem[] Items { get; set; }
    }

    public class ApiBasketItem
    {
        [JsonProperty("productId")]
        public string ProductId { get; set; }
        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }
}
