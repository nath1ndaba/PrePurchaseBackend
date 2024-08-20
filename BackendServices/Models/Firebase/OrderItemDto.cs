namespace BackendServices.Models.Firebase;
public class OrderItemDto
{
    public string ItemId { get; set; }
    public string Item { get; set; }
    public string ItemImage { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }
}

