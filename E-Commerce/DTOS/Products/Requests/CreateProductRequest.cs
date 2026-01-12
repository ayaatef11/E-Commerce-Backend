namespace E_Commerce.DTOS.Products.Requests
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public decimal Gomla { get; set; }
        public decimal GomlaPrice { get; set; }
        public decimal ListPrice { get; set; }
        public decimal Mandop { get; set; }
        public string PictureUrl { get; set; } = string.Empty;
        public int StockQuantity { get; set; }

    }
}
