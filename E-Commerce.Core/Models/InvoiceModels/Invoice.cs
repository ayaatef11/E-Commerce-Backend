using E_Commerce.Core.Shared.Utilties.Enums;

namespace E_Commerce.Core.Models.InvoiceModels;
public class Invoice : BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public DateTimeOffset OrderDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string UserEmail { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string UserPhoneNumber {  get; set; } = null!;
    public string UserAddress { get; set; } = null!;
    public string InvoiceNumber { get; set; } = null!;
    public bool IsPaid { get; set; }
    public DateTimeOffset PaymentDate { get; set; }
    public string? PaymentMethod { get; set; }
    public Status InvoiceStatus { get; set; }
}
