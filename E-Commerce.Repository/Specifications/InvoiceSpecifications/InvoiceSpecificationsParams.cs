namespace E_Commerce.Repository.Specifications.InvoiceSpecifications;
    public class InvoiceSpecificationsParams
    {
        public int Id { get; set; }
        private const int MaxPageSize = 10;

        private int pageSize = 5;
        public int PageIndex { get; set; } = 1;
        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = value > MaxPageSize ? pageSize : value; }
        }
        public string? Sort { get; set; }
        public string? SearchTerm { get; set; }
        public int? OrderId { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? UserName { get; set; }
    }

