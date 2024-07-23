using BackendServices;
using BackendServices.Actions.Inventory;
using BackendServices.Exceptions;
using BackendServices.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using PrePurchase.Models;
using PrePurchase.Models.Inventory;
using PrePurchase.Models.PrePurchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class OrderItemsActions : IOrderItemsActions
    {
        private readonly IRepository<OrderItem> _orderItemsRepository;
        private readonly ICommon _common;

        public OrderItemsActions(IRepository<OrderItem> orderItemsRepository, ICommon common)
        {
            _orderItemsRepository = orderItemsRepository ?? throw new ArgumentNullException(nameof(orderItemsRepository));
            _common = common ?? throw new ArgumentNullException(nameof(common));
        }

        public async Task<Response> GetOrderItems(string role, string companyId)
        {
            try
            {
                Shop shop = await _common.ValidateOwner<Shop>(role, companyId);
                IEnumerable<OrderItem> orderItems = await _orderItemsRepository.Find(u => u.ShopId == shop.Id);
                if (orderItems == null || !orderItems.Any())
                    throw new HttpResponseException($"No orderItems found for {shop.Name}");
                return new Response<IEnumerable<OrderItem>>(orderItems);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> AddOrderItem(string createdBy, string updatedBy, OrderItem orderItems, string role, string? companyId = null)
        {
            try
            {
                Shop shop = await _common.ValidateOwner<Shop>(role, companyId);
                OrderItem existingOrderItems = await _orderItemsRepository.FindOne(u => u.OrderID == orderItems.OrderID && u.ShopId == shop.Id);
                if (existingOrderItems != null)
                    throw new HttpResponseException($"OrderItems with id '{orderItems.OrderID}' already exists!");

                orderItems.CreatedBy = ObjectId.Parse(createdBy);
                orderItems.UpdatedBy = ObjectId.Parse(updatedBy);
                orderItems.CreateDate = DateTime.UtcNow;
                orderItems.UpdateDate = DateTime.UtcNow;
                orderItems.DeletedIndicator = false;
                orderItems.ShopId = shop.Id;

                await _orderItemsRepository.Insert(orderItems);

                return new Response(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> UpdateOrderItem(string updatedBy, OrderItem orderItems, string role, string? companyId = null)
        {
            try
            {
                Shop shop = await _common.ValidateOwner<Shop>(role, companyId);
                OrderItem existingOrderItem = await _orderItemsRepository.FindById(orderItems.Id.ToString());
                if (existingOrderItem == null || existingOrderItem.ShopId != shop.Id)
                    throw new HttpResponseException("OrderItems not found");

                existingOrderItem.UnitPrice = orderItems.UnitPrice;
                existingOrderItem.Quantity = orderItems.Quantity;
                existingOrderItem.ProductID = orderItems.ProductID;

                existingOrderItem.UpdatedBy = ObjectId.Parse(updatedBy);
                existingOrderItem.UpdateDate = DateTime.UtcNow;

                await _orderItemsRepository.Update(existingOrderItem.Id.ToString(), existingOrderItem);

                return new Response<OrderItem>(existingOrderItem);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> GetOrderItem(string id, string role, string? companyId = null)
        {
            try
            {
                Shop shop = await _common.ValidateOwner<Shop>(role, companyId);
                OrderItem orderItems = await _orderItemsRepository.FindById(id);
                if (orderItems == null || orderItems.ShopId != shop.Id)
                    throw new HttpResponseException("OrderItems not found");
                return new Response<OrderItem>(orderItems);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> SoftDeleteOrderItem(string updatedBy, string id, string role, string? companyId = null)
        {
            try
            {
                Shop shop = await _common.ValidateOwner<Shop>(role, companyId);
                OrderItem orderItems = await _orderItemsRepository.FindById(id);
                if (orderItems == null || orderItems.ShopId != shop.Id)
                    throw new HttpResponseException("OrderItems not found");

                orderItems.DeletedIndicator = true;
                orderItems.UpdatedBy = ObjectId.Parse(updatedBy);
                orderItems.UpdateDate = DateTime.UtcNow;

                await _orderItemsRepository.Update(orderItems.Id.ToString(), orderItems);

                return new Response<OrderItem>(orderItems);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

    }
}
