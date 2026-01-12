namespace E_Commerce.Core.Models.OrderModels;
public class Order : BaseEntity
{
    public string BuyerEmail { get; init; } = null!;
    public string BuyerName { get; set; } = null!;
    public string BuyerPhoneNumber { get; set; }=null!;
    public string BuyerAddress { get; set; } = null!;   
    public DateTimeOffset OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public ICollection<OrderItem>? Items { get; set; }
    public decimal Price { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset DeletedDate { get; set; }
}

