
using E_Commerce.DTOS.Cart.Responses;
using E_Commerce.Application.Interfaces.Core;
using E_Commerce.Core.Shared.Utilties.Identity;
namespace E_Commerce.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
public class CartController(ICartService _cartService, IMapper _mapper) : ControllerBase
{
    [HttpGet("get")]
    public async Task<ActionResult<CartResponse>> GetCart()
    {
        var result = await _cartService.GetUserCartAsync();
        if (!result.IsSuccess)
            return Problem(
               title: result.Error!.Title,
               detail: result.Error.Message,
               statusCode: result.Error.StatusCode
           );
        var cartResponse = _mapper.Map<CartResponse>(result.Value);
        return Ok(cartResponse);
    }

    [HttpPost("add/item")]
    public async Task<ActionResult<CartResponse>> AddItemToCart(int productId, int quantity)
    {
        var result = await _cartService.AddItemToCartAsync(productId, quantity);
        if (!result.IsSuccess)
            return Problem(
               title: result.Error!.Title,
               detail: result.Error.Message,
               statusCode: result.Error.StatusCode
           );
        var cartResponse = _mapper.Map<CartResponse>(result.Value);
        return Ok(cartResponse);
    }

    [HttpPut("update/item/{productId}")]
    public async Task<ActionResult<CartResponse>> UpdateItemQuantity(int productId, int newQuantity)
    {
        var result = await _cartService.UpdateCartItemQuantityAsync(productId, newQuantity);
        if (!result.IsSuccess)
            return Problem(
               title: result.Error!.Title,
               detail: result.Error.Message,
               statusCode: result.Error.StatusCode
           );
        var cartResponse = _mapper.Map<CartResponse>(result.Value);
        return Ok(cartResponse);
    }

    [HttpDelete("delete/item/{productId}")]
    public async Task<ActionResult<CartResponse>> RemoveItemFromCart(int productId)
    {
        var result = await _cartService.RemoveItemFromCartAsync(productId);
        if (!result.IsSuccess)
            return Problem(
               title: result.Error!.Title,
               detail: result.Error.Message,
               statusCode: result.Error.StatusCode
           );
        var cartResponse = _mapper.Map<CartResponse>(result.Value);
        return Ok(cartResponse);
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var result = await _cartService.ClearCartAsync();
        if (!result.IsSuccess)
            return Problem(
               title: result.Error!.Title,
               detail: result.Error.Message,
               statusCode: result.Error.StatusCode
           );
        return Ok("Cart is now Empty");
    }

    [HttpGet("total")]
    public async Task<IActionResult> GetCartTotal()
    {
        var result = await _cartService.CalculateCartTotalAsync();
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(
           title: result.Error!.Title,
           detail: result.Error.Message,
           statusCode: result.Error.StatusCode
       );
    }

}





