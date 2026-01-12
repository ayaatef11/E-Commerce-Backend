namespace E_Commerce.Core.Models.OrderModels;
    public enum OrderStatus
    {
        [EnumMember(Value = "Pending")]
        Pending,
        [EnumMember(Value = "Payment Succeeded")]
        PaymentSucceeded,
        [EnumMember(Value = "Payment Failed")]
        PaymentFailed,
        Canceled,
        Paid,
        Completed
    }


