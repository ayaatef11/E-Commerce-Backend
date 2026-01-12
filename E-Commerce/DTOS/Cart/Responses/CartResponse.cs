namespace E_Commerce.DTOS.Cart.Responses;
public class CartResponse
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public DateTime LastUpdated { get; set; }
    public List<CartItemResponse>? Items { get; set; } 
}