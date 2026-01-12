namespace E_Commerce.DTOS.Order.Responses;
public class OrderResponse
{
    public int Id { get; set; }
    public string BuyerEmail { get; init; }
    public string BuyerName { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public ICollection<OrderItemResponse> Items { get; set; }
    public decimal Price { get; set; } 
}

