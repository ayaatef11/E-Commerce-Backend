using E_Commerce.Core.Shared.Results;

namespace E_Commerce.Application.Interfaces.Core;
public interface IProductService
{ 
     Task<Result> CreateProductAsync(Product product);

     Task<Result<Product>> GetProductByIdAsync(int id);

     Task<Result<IReadOnlyList<Product>>> GetAllProductsAsync(int pageSize  , int pageIndex  );
    Task<Result<IReadOnlyList<Product>>> GetAllProductsAsync();

     Task<Result<Product>> UpdateProductAsync(int productId, Product updatedProduct);

     Task<Result> DeleteProductAsync(int id);
     Task<Result> ArchiveProductAsync(int id);

     Task<Result> UnarchiveProductAsync(int id);

     Task<Result<IReadOnlyList<Product>>> SearchProductsAsync(string keyword); 
}


