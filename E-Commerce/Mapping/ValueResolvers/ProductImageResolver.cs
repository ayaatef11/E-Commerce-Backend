using E_Commerce.DTOS.Products.Responses;

namespace E_Commerce.Mapping.ValueResolvers;
public class ProductImageCoverResolver(IConfiguration configuration) : IValueResolver<Product, ProductResponse, string>
{
    public string Resolve(Product source, ProductResponse destination, string destMember, ResolutionContext context)
    {
        return !string.IsNullOrEmpty(source.PictureUrl) ? $"{configuration["ApiBaseUrl"]}/{source.PictureUrl}" : string.Empty;
    }
}

