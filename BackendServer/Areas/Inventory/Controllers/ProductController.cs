﻿using BackendServices;
using BackendServices.Actions.Inventory;
using BackendServices.Exceptions;
using BackendServices.JWT;
using BackendServices.Models;
using BackendServices.Models.Inventory;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrePurchase.Models;
using PrePurchase.Models.Inventory;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BackendServer.V1.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Area("Inventory")]
    [Route("[area]/[controller]")]
    [ProducesResponseType(typeof(Response), 400)]
    [ProducesResponseType(typeof(Response), 500)]
    [ApiController]
    public class ProductController : BaseController
    {
        private readonly IProductsActions actions;
        public ProductController(IProductsActions actions, IAuthContainerModel containerModel, IAuthService authService, IRepository<RefreshToken> refreshTokens) : base(containerModel, authService, refreshTokens)
        {
            this.actions = actions;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Response<Product>), 200)]
        public async Task<Response> AddProduct([FromBody] ProductDto model, [FromQuery] string shopId = null)
        {
            
            string createdBy = User.FindFirstValue(ClaimTypes.Name);
            string updatedBy = createdBy;

            Response response = await actions.AddProduct(createdBy, updatedBy, model, shopId);

            if (response is not Response<Product> correspondingResponse) return response;

            return new Response<Product>(correspondingResponse.Data!);
        }

        [HttpPut]
        [ProducesResponseType(typeof(Response<Product>), 200)]
        public async Task<Response> UpdateProduct([FromBody] Product model, [FromQuery] string shopId = null)
        {
            
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);


            Response response = await actions.UpdateProduct(updatedBy, model, shopId);

            if (response is not Response<Product> correspondingResponse) return response;

            return new Response<Product>(correspondingResponse.Data!);
        }

        [HttpGet("{shopId}")]
        [ProducesResponseType(typeof(Response<ProductDto>), 200)]
        public async Task<Response> GetProducts([FromRoute] string shopId)
        {
            

            return await actions.GetProducts(shopId);
        }

        [HttpGet("productsforcategory")]
        [ProducesResponseType(typeof(Response<Product>), 200)]
        public async Task<Response> GetProductsForCategory([FromQuery] string categoryId, [FromQuery] string shopId)
        {
            

            return await actions.GetProductsForCategory(categoryId, shopId);
        }

        /*   [HttpGet("{id}")]
           [ProducesResponseType(typeof(Response<List<Product>>), 200)]
           public async Task<Response> GetProduct([FromRoute] string id, [FromQuery] string shopId = null)
           {
               
               

               return await actions.GetProduct(id,  shopId);
           }*/

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Response<List<Product>>), 200)]
        public async Task<Response> SoftDeleteProduct([FromRoute] string id, [FromQuery] string shopId = null)
        {
            
            string updatedBy = User.FindFirstValue(ClaimTypes.Name);


            return await actions.SoftDeleteProduct(updatedBy, id, shopId);
        }

        private string GetRole()
        {
            var accpetedRoles = new string[] { AuthRoles.Manager, AuthRoles.Owner }; // the user will have at least one of these roles before this functiion is executed

            return User.FindAll(x => x.Type == ClaimTypes.Role).FirstOrDefault(x => accpetedRoles.Contains(x.Value)).Value;

        }

#nullable enable
        private string? GetId(string? memberId = null)
        {
            var role = GetRole();

            if (role == AuthRoles.Owner)
                return Id;

            return memberId;
        }

        private void RequireProductId(string memberId)
        {
            var role = GetRole();
            if (role == AuthRoles.Manager && string.IsNullOrWhiteSpace(memberId))
                throw new HttpResponseException(new Response(error: "Product id is not specified!")); ;

        }

#nullable disable

    }
}
