namespace E_Commerce.DTOS.Order.Requests;
public class OrderItemRequest
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}