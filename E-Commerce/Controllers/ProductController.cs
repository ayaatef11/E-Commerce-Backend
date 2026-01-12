
using E_Commerce.DTOS.Auth.Responses;
using E_Commerce.DTOS.Products.Requests;
using E_Commerce.Application.Interfaces.Core;
using E_Commerce.Core.Shared.Utilties.Identity;
using E_Commerce.Repository.Repositories.Interfaces;
namespace E_Commerce.Controllers;
[Route("api/[controller]")]
    [ApiController]
    public class ProductController(IProductService _productService,IMapper _mapper,IUnitOfWork _unitOfWork) : ControllerBase
    {

    [Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAllProducts(
     [FromQuery] int pageSize = 10,
     [FromQuery] int pageIndex = 1)
    {
        var result = await _productService.GetAllProductsAsync(pageSize, pageIndex);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.Error.StatusCode, result.Error);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("filterPrices")]
    public async Task<IActionResult> FilterPrices([FromQuery] string filterType,[FromQuery] int pageSize = 10,[FromQuery] int pageIndex = 1)
    {
        var AllproductsResult = await _productService.GetAllProductsAsync();

        if (!AllproductsResult.IsSuccess)
        {
            return StatusCode(AllproductsResult.Error.StatusCode, AllproductsResult.Error);
        }

        filterType = filterType.ToLower();

        switch (filterType)
        {
            case "gomla":
                foreach (var product in AllproductsResult.Value)
                {
                    decimal pc = product.GomlaPrice;
                    product.Price = pc;
                    _unitOfWork.Repository<Product>().Update(product);
                    await _unitOfWork.CompleteAsync();
                }
                break;
            case "mandop":
                foreach (var product in AllproductsResult.Value)
                {
                    decimal pc = product.Mandop;
                    product.Price = pc;
                    _unitOfWork.Repository<Product>().Update(product);
                    await _unitOfWork.CompleteAsync();
                }
                break;
            case "list":
                foreach (var product in AllproductsResult.Value)
                {
                    decimal pc = product.ListPrice;
                    product.Price = pc;
                    _unitOfWork.Repository<Product>().Update(product);
                    await _unitOfWork.CompleteAsync();
                }
                break;
            default:
                return BadRequest(new Error("Filter.Invalid", "Invalid filter type", 400));
        }
        var productsResult = await _productService.GetAllProductsAsync(pageSize, pageIndex);
        return Ok(productsResult);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("Create")] 
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest productDto)
    {
        var product = _mapper.Map<Product>(productDto);
        var result = await _productService.CreateProductAsync(product);

        if (result.IsSuccess)
        {
            return Ok(product);
        }
        return StatusCode(result.Error.StatusCode, result.Error);
    }
    [Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
    [HttpGet("Get/{id}")] 
    public async Task<IActionResult> GetProductById(int id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return StatusCode(result.Error.StatusCode, result.Error);
    }
 
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] CreateProductRequest updateDto)
    {
        var existingResult = await _productService.GetProductByIdAsync(id);
        if (!existingResult.IsSuccess)
        {
            return StatusCode(existingResult.Error.StatusCode, existingResult.Error);
        }

        var product = _mapper.Map(updateDto, existingResult.Value);
        var updateResult = await _productService.UpdateProductAsync(id, product);

        return updateResult.IsSuccess ? Ok(updateResult.Value) : StatusCode(updateResult.Error.StatusCode, updateResult.Error);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await _productService.DeleteProductAsync(id);
        if (result.IsSuccess)
        {
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Product deleted successfully"
            });
        }

        return BadRequest( new ApiResponse
        {
            Success = false,
            Message = result.Error.Message ?? "An error occurred while deleting the product"
        });
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPatch("{id}/archive")] 
    public async Task<IActionResult> ArchiveProduct(int id)
    {
        var result = await _productService.ArchiveProductAsync(id);
        return result.IsSuccess ? NoContent() : StatusCode(result.Error.StatusCode, result.Error);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPatch("{id}/unarchive")] 
    public async Task<IActionResult> UnarchiveProduct(int id)
    {
        var result = await _productService.UnarchiveProductAsync(id);
        return result.IsSuccess ? NoContent() : StatusCode(result.Error.StatusCode, result.Error);
    }

    [Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
    [HttpGet("Search")] 
    public async Task<IActionResult> SearchProducts([FromQuery] string parameters)
    {
        var result = await _productService.SearchProductsAsync(parameters);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.Error.StatusCode, result.Error);
    }


}

