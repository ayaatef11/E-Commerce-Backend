namespace E_Commerce.Repository.Specifications.ProductSpecifications;
    public class ProductSearchParameters
    {
    public string SearchParam { get; set; } = "Name";
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    }

