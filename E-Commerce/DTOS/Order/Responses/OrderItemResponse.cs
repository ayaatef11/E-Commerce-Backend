namespace E_Commerce.DTOS.Order.Responses
{
    public class OrderItemResponse
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; } 
    }
}
