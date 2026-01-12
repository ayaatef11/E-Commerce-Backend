
using E_Commerce.Application.Interfaces.Core;
using E_Commerce.Core.Shared.Utilties.Identity;

namespace E_Commerce.Controllers;
[Route("api/[controller]")]
[ApiController]
public class OrderController(IOrderService _orderService, IMapper _mapper, ICartService _cartService) : ControllerBase
{
    [Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
    [HttpPost("CreateOrder")]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest orderDto)
    {
        var order = _mapper.Map<Order>(orderDto);
        var result = await _orderService.CreateOrder(order);
        if (!result.IsSuccess)
        {
            return Problem(
                title: result.Error.Title,
                statusCode: result.Error.StatusCode
            );
        }
        var orderReturnDto = _mapper.Map<OrderResponse>(order);
        return Ok(orderReturnDto);
    }
  
    [Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
    [HttpPost("Create-Order-From-Cart")]
    public async Task<ActionResult<OrderResponse>> CreateOrderFromCart()
    {
        var result = await _cartService.CreateOrderFromCartAsync();
        if (!result.IsSuccess)
        {
            return Problem(
                title: result.Error.Title,
                statusCode: result.Error.StatusCode
            );
        }
        var orderResponse = _mapper.Map<OrderResponse>(result.Value);

        return Ok(orderResponse);
    }
   
    [Authorize(Roles = $"{Roles.User},{Roles.Admin}")]  
    [HttpGet("{id}")] 
    public async Task<ActionResult<OrderResponse>> GetOrder(int id)
        {
            var buyerEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(buyerEmail))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Authentication required",
                    Detail = "User email claim missing",
                    Status = StatusCodes.Status401Unauthorized
                });
            }

            var result = await _orderService.GetById(id, buyerEmail);

        if (!result.IsSuccess)
        {
            return Problem(
                title: result.Error.Title,
                statusCode: result.Error.StatusCode
            );
        }

        var orderResponse = _mapper.Map<OrderResponse>(result.Value);

        return Ok(orderResponse);
    }
    [Authorize(Roles = Roles.Admin)]  
    [HttpGet("Admin/{id}")]
    public async Task<ActionResult<OrderResponse>> AdminGetOrder(int id)
    {  
        var result = await _orderService.GetById(id);

        if (!result.IsSuccess)
        {
            return Problem(
                title: result.Error.Title,
                statusCode: result.Error.StatusCode
            );
        }

        var orderResponse = _mapper.Map<OrderResponse>(result.Value);

        return Ok(orderResponse);
    }

    [Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
    [HttpPost("{orderId}/addItem")]
    public async Task<ActionResult<OrderResponse>> AddProductToOrder(int orderId, [FromBody] OrderItemRequest itemDto)
    {
        var item = _mapper.Map<OrderItem>(itemDto);
        var result = await _orderService.AddProductToOrder(orderId, item);
        if (!result.IsSuccess)
        {
            return Problem(
                title: result.Error.Title,
                statusCode: result.Error.StatusCode
            );
        }
        var orderResponse = _mapper.Map<OrderResponse>(result.Value);

        return Ok(orderResponse);

    }

    [Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
    [HttpPut("{orderId}/updateItem")]
    public async Task<ActionResult<OrderResponse>> UpdateProductInOrder(int orderId, int itemId, int newQuantity)
    {
        var result = await _orderService.UpdateProductInOrder(orderId, itemId, newQuantity);
        if (!result.IsSuccess)
        {
            return Problem(
                title: result.Error.Title,
                statusCode: result.Error.StatusCode
            );
        }
        var orderResponse = _mapper.Map<OrderResponse>(result.Value);

        return Ok(orderResponse);
    }

    [Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
    [HttpDelete("{orderId}/deleteItem")]
    public async Task<ActionResult<OrderResponse>> RemoveProductFromOrder(int orderId, int itemId)
    {
        var result = await _orderService.RemoveProductFromOrder(orderId, itemId);
        if (!result.IsSuccess)
        {
            return Problem(
                title: result.Error.Title,
                statusCode: result.Error.StatusCode
            );
        }
        var orderResponse = _mapper.Map<OrderResponse>(result.Value);

        return Ok(orderResponse);
    }

    [Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
    [HttpGet("{orderId}/getStatus")]
    public async Task<ActionResult<string>> GetOrderStatus(int orderId)
    {
        var status = await _orderService.TrackOrderStatus(orderId);
        return Ok(status);
    }

    [Authorize(Roles = $" {Roles.Admin}")]
    [HttpPut("{orderId}/UpdateStatus")]
    public async Task<ActionResult<OrderResponse>> UpdateOrderStatus(int orderId, [FromBody] string newStatus)
    {
        var result = await _orderService.UpdateOrderStatus(orderId, newStatus);
        if (!result.IsSuccess)
        {
            return Problem(
                title: result.Error.Title,
                statusCode: result.Error.StatusCode
            );
        }
        var orderResponse = _mapper.Map<OrderResponse>(result.Value);

        return Ok(orderResponse);
    }

    [Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
    [HttpPost("{orderId}/cancel")]
    public async Task<ActionResult<OrderResponse>> CancelOrder(int orderId, string email)
    {
        var result = await _orderService.CancelOrder(orderId, email);
        if (!result.IsSuccess)
        {
            return Problem(
                title: result.Error.Title,
                statusCode: result.Error.StatusCode
            );
        }
        var orderResponse = _mapper.Map<OrderResponse>(result.Value);

        return Ok(orderResponse);
    }

    [Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
    [HttpGet("sorted")]
    public async Task<ActionResult<List<OrderResponse>>> GetOrdersSortedByDate(int page,int pageSize,
     bool descending = true)
    {
        var buyerEmail = User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(buyerEmail) && !User.IsInRole(Roles.Admin))
        {
            return Unauthorized();
        }

        var result = await _orderService.GetOrdersSortedByDateAsync(page,pageSize,descending, buyerEmail);

        if (!result.IsSuccess)
        {
            return Problem(
                title: result.Error.Title,
                statusCode: result.Error.StatusCode
            );
        }

        return Ok(_mapper.Map<List<OrderResponse>>(result.Value));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin/sorted")]
    public async Task<ActionResult<List<OrderResponse>>> AdminGetOrdersSortedByDate(int page,int pageSize,
          bool descending = true)
    {
        var result = await _orderService.GetOrdersSortedByDateAsync(page,pageSize,descending);

        if (!result.IsSuccess)
        {
            return Problem(
                title: result.Error.Title,
                statusCode: result.Error.StatusCode
            );
        }

        return Ok(_mapper.Map<List<OrderResponse>>(result.Value));
    }
}


