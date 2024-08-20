using BackendServices;
using BackendServices.Actions.PrePurchase;
using BackendServices.JWT;
using BackendServices.Models;
using BackendServices.Models.Firebase;
using BackendServices.Models.PrePurchase;
using Infrastructure.Firebase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrePurchase.Models;
using PrePurchase.Models.PrePurchase;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackendServer.V1.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Area("prepurchase")]
    [Route("[area]/[controller]")]
    [ApiController]

    public class PrePurchaseOrdersController : BaseController
    {
        [HttpPost]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> PlaceOrder([FromBody] OrderDto order)
        {
            Response response = await _orderProcessingService.ProcessOrderAsync(order);
            return response;
        }

        [HttpPut("{orderId}")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> UpdateOrderStatus(string orderId, [FromBody] string status)
        {
            Response response = await _orderProcessingService.UpdateOrderStatusAsync(orderId, status);
            return response;
        }


        public PrePurchaseOrdersController(
          IAuthContainerModel containerModel
          , IAuthService authService
          , IRepository<RefreshToken> refreshTokens,
            OrderProcessingService orderProcessingService)
          : base(containerModel, authService, refreshTokens)
        {
            _orderProcessingService = orderProcessingService;
        }
        private readonly OrderProcessingService _orderProcessingService;
        protected string UserId => User.FindFirstValue(PrePurchaseJwtConstants.UserId);
    }
}