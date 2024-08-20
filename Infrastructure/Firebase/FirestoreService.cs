namespace Infrastructure.Firebase;
using BackendServices.Models.Firebase;
using BackendServices.Models.PrePurchase;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FirestoreService
{
    private readonly FirestoreDb _firestoreDb;

    public FirestoreService()
    {
        _firestoreDb = FirestoreDb.Create("pre-purchase");
    }

    public async Task<ShopDto> GetShopAsync(string shopId)
    {
        var documentReference = _firestoreDb.Collection("shops").Document(shopId);
        var snapshot = await documentReference.GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<ShopDto>() : null;
    }

    public async Task<UserDto> GetUserAsync(string userId)
    {
        var documentReference = _firestoreDb.Collection("users").Document(userId);
        var snapshot = await documentReference.GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<UserDto>() : null;
    }

    public async Task AddOrderAsync(OrderDto order)
    {
        var documentReference = _firestoreDb.Collection("orders").Document(order.OrderId);
        await documentReference.SetAsync(order);
    }

    public async Task<OrderDto?> GetOrderAsync(string orderId)
    {
        var documentReference = _firestoreDb.Collection("orders").Document(orderId);
        var snapshot = await documentReference.GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<OrderDto>() : null;
    }
    public async Task UpdateOrderAsync(string orderId, OrderDto order)
    {
        var documentReference = _firestoreDb.Collection("orders").Document(orderId);
        await documentReference.SetAsync(order, SetOptions.MergeAll);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersForShopAsync(string shopId)
    {
        var query = _firestoreDb.Collection("orders").WhereEqualTo("ShopId", shopId);
        var snapshot = await query.GetSnapshotAsync();
        var orders = new List<OrderDto>();
        foreach (var document in snapshot.Documents)
        {
            orders.Add(document.ConvertTo<OrderDto>());
        }
        return orders;
    }

    public void ListenForOrderUpdates(string orderId, Action<DocumentSnapshot> onUpdate)
    {
        var documentReference = _firestoreDb.Collection("orders").Document(orderId);
        documentReference.Listen(snapshot =>
        {
            if (snapshot.Exists)
            {
                onUpdate(snapshot);
            }
        });
    }
}


