namespace E_Commerce.DTOS.Invoice.Responses;
    public class InvoiceItemResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }

