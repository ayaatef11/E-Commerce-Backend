namespace E_Commerce.Core.Models.CartModels;
public class CartItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string PictureUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}


