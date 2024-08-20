using System;
using System.Collections.Generic;

namespace BackendServices.Models.Firebase;
public class OrderDto
{
    public string OrderId { get; set; }
    public string ShopId { get; set; }
    public string UserId { get; set; }
    public List<OrderItemDto> Items { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; }  // Added status field
}
