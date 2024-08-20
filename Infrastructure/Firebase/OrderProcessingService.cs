using BackendServices.Exceptions;
using BackendServices.Models;
using BackendServices.Models.Firebase;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Firebase;
public class OrderProcessingService
{
    private readonly FirestoreService _firestoreService;
    private readonly NotificationService _notificationService;

    public OrderProcessingService(FirestoreService firestoreService, NotificationService notificationService)
    {
        _firestoreService = firestoreService;
        _notificationService = notificationService;
    }

    public async Task<Response> ProcessOrderAsync(OrderDto order)
    {
        // Save the order to Firestore
        await _firestoreService.AddOrderAsync(order);

        // Retrieve the shop's token
        var shopToken = await GetShopTokenAsync(order.ShopId);

        string message = string.Empty;
        // Send notification to the shop
        if (!string.IsNullOrEmpty(shopToken))
        {
            message = $"Order {order.OrderId} has been placed.";
            await _notificationService.SendNotificationAsync(shopToken, "New Order", message);
        }

        // Retrieve the resident's token
        var residentToken = await GetResidentTokenAsync(order.UserId);

        // Send notification to the resident
        if (!string.IsNullOrEmpty(residentToken))
        {
            message = "Your order has been placed successfully.";
            await _notificationService.SendNotificationAsync(residentToken, "Order Placed", message);
        }
        return new Response(HttpStatusCode.Created, message);
    }

    public async Task<Response> UpdateOrderStatusAsync(string orderId, string status)
    {
        var order = await _firestoreService.GetOrderAsync(orderId);
        if (order != null)
        {
            order.Status = status;
            await _firestoreService.UpdateOrderAsync(orderId, order);
            // Notify both shop and resident
            var shopToken = await GetShopTokenAsync(order.ShopId);
            var residentToken = await GetResidentTokenAsync(order.UserId);

            string message = string.Empty;
            if (!string.IsNullOrEmpty(shopToken))
            {
                message = $"Order {orderId} has been {status}.";
                await _notificationService.SendNotificationAsync(shopToken, "Order Status Update", message);
            }

            if (!string.IsNullOrEmpty(residentToken))
            {
                message = $"Your order {orderId} has been {status}.";
                await _notificationService.SendNotificationAsync(residentToken, "Order Status Update", message);
            }
            return new Response(HttpStatusCode.Created, message);
        }
        throw new HttpResponseException($"No orders found for {orderId}");
    }

    private async Task<string> GetShopTokenAsync(string shopId)
    {
        var shop = await _firestoreService.GetShopAsync(shopId);
        return shop?.NotificationToken;
    }

    private async Task<string> GetResidentTokenAsync(string userId)
    {
        var user = await _firestoreService.GetUserAsync(userId);
        return user?.NotificationToken;
    }
}

