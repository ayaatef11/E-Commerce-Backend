namespace E_Commerce.Repository.Specifications.ProductSpecifications;
    public class ProductSpecificationParameters
    {
        private const int MaxPageSize = 10;
        private int pageSize = 5;
        public int PageIndex { get; set; } = 1;
        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = value > MaxPageSize ? pageSize : value; }
        }
        public string? Sort { get; set; }
        public string? Search { get; set; }
    }
