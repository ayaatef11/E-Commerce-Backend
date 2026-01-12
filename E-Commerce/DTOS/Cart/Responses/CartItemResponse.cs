namespace E_Commerce.DTOS.Cart.Responses;
public class CartItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string PictureUrl { get; set; }
}

