using E_Commerce.Repository.Specifications.ProductSpecifications;
using E_Commerce.Application.Interfaces.Core;
using E_Commerce.Infrastructure.Persistence.Seeding.ExcelParser;
using E_Commerce.Repository.Repositories.Interfaces;
using E_Commerce.Repository.Specifications.ProductSpecifications;
namespace E_Commerce.Application.Services.Core
{
    public class ProductService(IUnitOfWork _unitOfWork, IImportProductData _importProductData) : IProductService
    {
        public async Task<Result> CreateProductAsync(Product product)
        {
            if (product == null)
                return Result.Failure(new Error
                {
                    Title = "Product cannot be null",
                    StatusCode = StatusCodes.Status404NotFound
                });
        
            try
            {
                await _unitOfWork.Repository<Product>().AddAsync(product);
                var changes = await _unitOfWork.CompleteAsync();
                return changes > 0
                    ? Result.Success("Product created successfully")
                    : Result.Failure(Error.DatabaseError("No changes persisted"));
            }
            catch (Exception ex)
            {
                return Result.Failure(Error.DatabaseError(ex.Message));
            }
        }

        public async Task<Result<Product>> GetProductByIdAsync(int id)
        {
            if (id <= 0)
                return Result.Failure<Product>(new Error("Product.InvalidId", "Invalid product ID", 400));

            try
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
                return product != null
                    ? Result.Success(product)
                    : Result.Failure<Product>(Error.ProductNotFound(id));
            }
            catch (Exception ex)
            {
                return Result.Failure<Product>(Error.DatabaseError(ex.Message));
            }
        }

        public async Task<Result<IReadOnlyList<Product>>> GetAllProductsAsync(int pageSize  , int pageIndex)
        {
            if (pageSize <= 0 || pageIndex <= 0)
                return Result.Failure<IReadOnlyList<Product>>(
                    new Error("Pagination.Invalid", "Page size and index must be greater than 0", 400));

            try
            {
                var spec = new ProductSpecification(pageSize, pageIndex);
                var products = await _unitOfWork.Repository<Product>().GetAllWithSpecAsync(spec);

                if (!products.Any())
                {
                    await _importProductData.ImportProductsFromExcel();
                    products = await _unitOfWork.Repository<Product>().GetAllWithSpecAsync(spec);
                }

                return Result.Success(products);
            }
            catch (Exception ex)
            {
                return Result.Failure<IReadOnlyList<Product>>(Error.DatabaseError(ex.Message));
            }
        }
        public async Task<Result<IReadOnlyList<Product>>> GetAllProductsAsync( )
        {
            
            try
            { 
                var products = await _unitOfWork.Repository<Product>().GetAllAsync();

                if (!products.Any())
                {
                    await _importProductData.ImportProductsFromExcel();
                    products = await _unitOfWork.Repository<Product>().GetAllAsync();
                }

                return Result.Success(products);
            }
            catch (Exception ex)
            {
                return Result.Failure<IReadOnlyList<Product>>(Error.DatabaseError(ex.Message));
            }
        }

        public async Task<Result<Product>> UpdateProductAsync(int productId, Product updatedProduct)
        {
            if (productId <= 0)
                return Result.Failure<Product>(new Error("Product.InvalidId", "Invalid product ID", 400));

            if (updatedProduct == null)
                return Result.Failure<Product>(new Error("Product.Null", "Updated product cannot be null", 400));

            try
            {
                var existingProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
                if (existingProduct == null)
                    return Result.Failure<Product>(Error.ProductNotFound(productId));

                if (updatedProduct.Id != productId)
                    return Result.Failure<Product>(new Error("Product.IdMismatch", "Product ID mismatch", 400));

                _unitOfWork.Repository<Product>().Update(updatedProduct);
                await _unitOfWork.CompleteAsync();
                return Result.Success(updatedProduct);
            }
            catch (Exception ex)
            {
                return Result.Failure<Product>(Error.DatabaseError(ex.Message));
            }
        }

        public async Task<Result> DeleteProductAsync(int id)
        {
            if (id <= 0)
                return Result.Failure(new Error("Product.InvalidId", "Invalid product ID", 400));

            try
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
                if (product == null)
                    return Result.Failure(Error.ProductNotFound(id));

                _unitOfWork.Repository<Product>().Delete(product);
                await _unitOfWork.CompleteAsync();
                return Result.Success("Product deleted successfully");
            }
            catch (Exception ex)
            {
                return Result.Failure(Error.DatabaseError(ex.Message));
            }
        }

        public async Task<Result> ArchiveProductAsync(int id)
        {
            if (id <= 0)
                return Result.Failure(new Error("Product.InvalidId", "Invalid product ID", 400));

            try
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
                if (product == null)
                    return Result.Failure(Error.ProductNotFound(id));

                product.IsDeleted = true;
                product.DeletedDate = DateTime.UtcNow;
                await _unitOfWork.CompleteAsync();
                return Result.Success("Product archived successfully");
            }
            catch (Exception ex)
            {
                return Result.Failure(Error.DatabaseError(ex.Message));
            }
        }

        public async Task<Result> UnarchiveProductAsync(int id)
        {
            if (id <= 0)
                return Result.Failure(new Error("Product.InvalidId", "Invalid product ID", 400));

            try
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdIgnoreFiltersAsync(id);
                if (product == null)
                    return Result.Failure(Error.ProductNotFound(id));

                product.IsDeleted = false;
                await _unitOfWork.CompleteAsync();
                return Result.Success("Product unarchived successfully");
            }
            catch (Exception ex)
            {
                return Result.Failure(Error.DatabaseError(ex.Message));
            }
        }

        public async Task<Result<IReadOnlyList<Product>>> SearchProductsAsync(string keyword)
        {
            if (keyword == null)
                return Result.Failure<IReadOnlyList<Product>>(
                    new Error("Search.Invalid", "Search parameters cannot be null", 400));

            try
            {
                var spec = new ProductSpecification(keyword);
                var products = await _unitOfWork.Repository<Product>().GetAllWithSpecAsync(spec);
                return Result.Success(products);
            }
            catch (Exception ex)
            {
                return Result.Failure<IReadOnlyList<Product>>(Error.DatabaseError(ex.Message));
            }
        }

        public async Task<Result<IReadOnlyList<Product>>> FilterProductsAsync(ProductFilterParameters filter)
        {
            if (filter == null)
                return Result.Failure<IReadOnlyList<Product>>(
                    new Error("Filter.Invalid", "Filter parameters cannot be null", 400));

            try
            {
                var spec = new ProductSpecification(filter);
                var products = await _unitOfWork.Repository<Product>().GetAllWithSpecAsync(spec);
                return Result.Success(products);
            }
            catch (Exception ex)
            {
                return Result.Failure<IReadOnlyList<Product>>(Error.DatabaseError(ex.Message));
            }
        }

        public async Task<Result<IReadOnlyList<Product>>> SortProductsAsync(ProductFilterParameters sortBy)
        {
            if (sortBy == null)
                return Result.Failure<IReadOnlyList<Product>>(
                    new Error("Sort.Invalid", "Sort parameters cannot be null", 400));

            try
            {
                var spec = new ProductSpecification(sortBy);
                var products = await _unitOfWork.Repository<Product>().GetAllWithSpecAsync(spec);
                return Result.Success(products);
            }
            catch (Exception ex)
            {
                return Result.Failure<IReadOnlyList<Product>>(Error.DatabaseError(ex.Message));
            }
        }
    }
}