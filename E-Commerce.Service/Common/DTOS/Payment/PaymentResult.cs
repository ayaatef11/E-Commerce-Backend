namespace E_Commerce.Application.Common.DTOS.Payment;
public class PaymentResult
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = null!;
    public string ErrorMessage { get; set; } = null!;
}