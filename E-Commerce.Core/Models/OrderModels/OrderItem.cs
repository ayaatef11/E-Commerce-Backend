namespace E_Commerce.Core.Models.OrderModels;
public class OrderItem : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total => Price * Quantity;
    public bool IsDeleted { get; set; }
    public DateTimeOffset DeletedDate { get; set; }
}


