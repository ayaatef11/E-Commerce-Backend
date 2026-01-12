namespace E_Commerce.Core.Models.CartModels;
public class Cart : BaseEntity
{
    public string UserId { get; set; } = null!;
    public DateTime LastUpdated { get; set; }
    public List<CartItem> Items { get; set; } = new List<CartItem>();

}


