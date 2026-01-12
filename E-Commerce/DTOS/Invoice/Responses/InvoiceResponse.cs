using E_Commerce.Core.Shared.Utilties.Enums;

namespace E_Commerce.DTOS.Invoice.Responses;
public class InvoiceResponse
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string UserEmail { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string UserPhoneNumber { get; set; } = null!;
    public string UserAddress {  get; set; } = null!;
    public string InvoiceNumber { get; set; } = null!;
    public bool IsPaid { get; set; }
    public string? PaymentMethod { get; set; }
    public Status InvoiceStatus { get; set; }
    public List<InvoiceItemResponse> Items { get; set; } = new();
}

