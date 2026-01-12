namespace E_Commerce.Repository.Specifications.ProductSpecifications;
public class ProductFilterParameters
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Price { get; set; } = null!;
    public string Sort { get; set; } = null!;
    public string Search { get; set; } = null!;
    private const int MaxPageSize = 10;

    private int pageSize = 5;
    public int PageIndex { get; set; } = 1;

    public int PageSize
    {
        get { return pageSize; }
        set { pageSize = value > MaxPageSize ? pageSize : value; }
    }
}
