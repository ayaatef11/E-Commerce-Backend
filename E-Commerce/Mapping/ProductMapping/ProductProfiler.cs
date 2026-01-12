using E_Commerce.DTOS.Products.Requests;
using E_Commerce.DTOS.Products.Responses;

namespace E_Commerce.Mapping.ProductMapping;
public class ProductProfiler:Profile
    {
    public ProductProfiler()
    {
        CreateMap<CreateProductRequest, Product>();
    }
    }

