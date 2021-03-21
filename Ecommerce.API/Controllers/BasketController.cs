﻿using Ecommerce.API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System;
using System.Linq;
using System.Threading.Tasks;
using UserActor.Interfaces;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BasketController : ControllerBase
    {
        [HttpGet("{userId}")]
        public async Task<ApiBasket> GetAsync(string userId)
        {
            var actor = GetActor(userId);
            var products = await actor.GetBasket();
            return new ApiBasket
            {
                UserId = userId,
                Items = products.Select(p => new ApiBasketItem
                {
                    ProductId = p.ProductId.ToString(),
                    Quantity = p.Quantity
                }).ToArray()
            };
        }

        [HttpPost("{userId}")]
        public async Task AddAsync(string userId, [FromBody] ApiBasketAddRequest request)
        {
            var actor = GetActor(userId);
            await actor.AddToBasket(request.ProductId, request.Quantity);
        }

        [HttpDelete("{userId}")]
        public async Task DeletetAsync(string userId)
        {
            var actor = GetActor(userId);
            await actor.ClearBasket();
        }

        private IUserActor GetActor(string userId)
        {
            return ActorProxy.Create<IUserActor>(
                new ActorId(userId),
                new Uri("fabric:/Ecommerce/UserActorService"));
        }
                
    }
}
