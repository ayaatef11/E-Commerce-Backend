namespace E_Commerce.Repository.Specifications.ProductSpecifications;
public class ProductSpecification : BaseSpecification<Product>
{
    public ProductSpecification(ProductSearchParameters specParams)
    {
        var searchTerm = specParams.SearchParam.ToLower();
        WhereCriteria = p =>

string.IsNullOrEmpty(specParams.SearchParam) ||

p.Name.ToLower().Contains(searchTerm) ||

p.Description.ToLower().Contains(searchTerm) ||

p.PictureUrl.ToLower().Contains(searchTerm) ||

p.Cost.ToString().Contains(searchTerm) ||

p.Gomla.ToString().Contains(searchTerm) ||

p.GomlaPrice.ToString().Contains(searchTerm) ||

p.ListPrice.ToString().Contains(searchTerm) ||

p.Mandop.ToString().Contains(searchTerm) ||

p.StockQuantity.ToString().Contains(searchTerm);

        ApplyPagination((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);

    }

    public ProductSpecification(ProductFilterParameters specParams)
    {
        WhereCriteria = p =>
      (string.IsNullOrEmpty(specParams.Name) || p.Name.ToLower().Contains(specParams.Name.ToLower())) &&
        (string.IsNullOrEmpty(specParams.Description) || p.Name.ToLower().Contains(specParams.Description.ToLower()));//&&


        if (specParams.Sort == "priceDesc")
            OrderBy = p => p.Cost;
        else if (specParams.Sort == "priceAsc")
            OrderByDesc = p => p.Cost;
        else
            OrderBy = p => p.Name;
        ApplyPagination((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);



    }
    public ProductSpecification(string specParams)
    {
        WhereCriteria = p =>
      (string.IsNullOrEmpty(specParams) || p.Name.ToLower().Contains(specParams)) &&
        (string.IsNullOrEmpty(specParams) || p.Name.ToLower().Contains(specParams));//&&



    }
    public ProductSpecification(int pageSize, int pageIndex)
    {
        ApplyPagination((pageIndex - 1) * pageSize, pageSize);

    }
}
