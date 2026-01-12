using E_Commerce.Core.Data;
using E_Commerce.Application.Interfaces.Authentication;
using E_Commerce.Application.Interfaces.Core;

namespace E_Commerce.Application.Services.Core;
public class CartService(StoreContext _context, IUserService _userService,UserManager<AppUser>_userManager) : ICartService
{
    public async Task<Result<Cart>> GetUserCartAsync()
    {
        var userId = _userService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Result.Failure<Cart>(new Error
            {
                Title = "User Not Authorized",
                StatusCode = StatusCodes.Status401Unauthorized
            });
        }

        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                Items = new List<CartItem>()  
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return Result.Success(cart);
    }

    public async Task<Result<Cart>> AddItemToCartAsync(int productId, int quantity)
    {
        if (productId <= 0)
        {
            return Result.Failure<Cart>(new Error
            {
                Title = "Product id is invalid  ",
                StatusCode = StatusCodes.Status400BadRequest
            });
        }
        if (quantity <= 0)
        {
            return Result.Failure<Cart>(new Error
            {
                Title = "invalid quantity",
                StatusCode = StatusCodes.Status400BadRequest
            });
        }
        var userId = _userService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Result.Failure<Cart>(new Error("Unauthorized", "User not authenticated", 401));

        var cartResult = await GetUserCartAsync();
        if (cartResult.IsFailure)
            return Result.Failure<Cart>(cartResult.Error);

        var product = await _context.Products.FirstOrDefaultAsync(p=>p.Id==productId);//why find async doesnt work
        if (product == null)
            return Result.Failure<Cart>(new Error("ProductNotFound", "Product does not exist", 404));

        if (product.StockQuantity < quantity)
            return Result.Failure<Cart>(new Error("InsufficientStock", "Not enough stock available", 400));

        var cart = cartResult.Value;
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = productId,
                Quantity = quantity,
                Price = product.Price,
                Name = product.Name,
                PictureUrl = product.PictureUrl
            });
            cart.LastUpdated = DateTime.Now;
        }
        try
        {
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
            return Result.Success(cart);
        }
        catch (Exception ex)
        {
            return Result.Failure<Cart>(new Error("DatabaseError", ex.Message, 500));
        }
    }
 
    public async Task<Result<Cart>> UpdateCartItemQuantityAsync(int productId, int newQuantity)
    {
        if (productId <= 0)
        {
            return Result.Failure<Cart>(new Error
            {
                Title = "Product id is invalid  ",
                StatusCode = StatusCodes.Status400BadRequest
            });
        }
        var cartResult = await GetUserCartAsync();
        if (cartResult.IsFailure)
            return cartResult;

        var cart = cartResult.Value;
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            return Result.Failure<Cart>(new Error
            {
                Title = "item not found",
                StatusCode = StatusCodes.Status404NotFound
            });
        if (newQuantity <= 0)
            return Result.Failure<Cart>(new Error
            {
                Title = "Quantity is invalid  ",
                StatusCode = StatusCodes.Status400BadRequest
            });
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return Result.Failure<Cart>(new Error
            {
                Title = "Product not found  ",
                StatusCode = StatusCodes.Status404NotFound
            });
        item.Quantity = newQuantity;
        try
        {
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
            return Result.Success(cart);
        }
        catch (Exception ex)
        {
            return Result.Failure<Cart>(new Error
            {
                Title = "Database Error ",
                StatusCode = StatusCodes.Status500InternalServerError
            });
        }
    } 
    public async Task<Result<Cart>> RemoveItemFromCartAsync(int id)
    {
        if (id <= 0)
        {

            return Result.Failure<Cart>(new Error
            {
                Title = "Product id is invalid  ",
                StatusCode = StatusCodes.Status400BadRequest
            });
        }
        var cartResult = await GetUserCartAsync();
        if (cartResult.IsFailure)
        {
            return Result.Failure<Cart>(cartResult.Error);
        }

        var cart = cartResult.Value;
        if (cart == null)
        {
            return Result.Failure<Cart>(new Error
            {
                Title = "Cart not found",
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User cart does not exist"
            });
        }

        var item = cart.Items.FirstOrDefault(i => i.Id == id);
        if (item == null)
        {
            return Result.Failure<Cart>(new Error
            {
                Title = "Item not found",
                StatusCode = StatusCodes.Status404NotFound,
                Message = $"Product {id} not found in cart"
            });
        }

        try
        {
            cart.Items.Remove(item);
            _context.Carts.Update(cart);
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return Result.Success(cart);
        }
        catch (Exception ex)
        {
            return Result.Failure<Cart>(Error.DatabaseError(ex.Message));
        }
    }

    public async Task<Result> ClearCartAsync()
    {
        var cartResult = await GetUserCartAsync();
        if (cartResult.IsFailure)
        {
            return Result.Failure<Order>(cartResult.Error);
        }

        var cart = cartResult.Value;
        if (cart == null)
        {
            return Result.Failure(new Error
            {
                Title = "Cart not found",
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User cart does not exist"
            });
        }

        try
        {
            cart.Items.Clear();
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.DatabaseError(ex.Message));
        }
    }

    public async Task<Result<decimal>> CalculateCartTotalAsync()
    {
        var cartResult = await GetUserCartAsync();
        if (cartResult.IsFailure)
        {
            return Result.Failure<decimal>(cartResult.Error);
        }
        var cart = cartResult.Value;
        if (cart == null || !cart.Items.Any())
        {
            return Result.Success(0m);
        }
        try
        {
            var total = cart.Items.Sum(i => i.Quantity * i.Product.Cost);
            return Result.Success(total);
        }
        catch (Exception ex)
        {
            return Result.Failure<decimal>(new Error
            {
                Title = "Calculation error",
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = $"Error calculating cart total: {ex.Message}"
            });
        }
    }
    public async Task<Result<Order>> CreateOrderFromCartAsync()
    {
        var cartResult = await GetUserCartAsync();
        if (cartResult.IsFailure)
        {
            return Result.Failure<Order>(cartResult.Error);
        }

        var userId = _userService.GetCurrentUserId();
        if (userId == null)
        {
            return Result.Failure<Order>(Error.Unauthorized);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure<Order>(Error.UserNotFound);
        }
        var cart = cartResult.Value;
        if (cart == null || cart.Items.Count == 0)
        {
            return Result.Failure<Order>(Error.EmptyCart);
        }
        foreach (var item in cart.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
            {
                return Result.Failure<Order>(Error.ProductNotFound(item.ProductId));
            }
            if (product.StockQuantity < item.Quantity)
            {
                return Result.Failure<Order>(Error.InsufficientStock(
                    item.ProductId,
                    product.StockQuantity,
                    item.Quantity
                ));
            }
        }

        var executionStrategy = _context.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    BuyerEmail = user.Email!,
                    BuyerName = user.Full_Name,
                    BuyerPhoneNumber=user.PhoneNumber??"_",
                    BuyerAddress=user.Address??"_",
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    Items = cart.Items.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price
                    }).ToList(),
                     Price = cart.Items.Sum(item => item.Price*item.Quantity)

                };

                foreach (var item in cart.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null) { continue; }
                    product.StockQuantity -= item.Quantity;

                    if (product.StockQuantity < 0)
                    {
                        return Result.Failure<Order>(Error.NegativeStock(item.ProductId));
                    }

                    _context.Products.Update(product);
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                cart.Items.Clear();
                _context.Carts.Update(cart);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var finalOrder = await _context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                return Result.Success(finalOrder);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result.Failure<Order>(Error.DatabaseError(ex.Message));
            }
        });
    }


}


