using E_Commerce.Core.Shared.Results;

namespace E_Commerce.Application.Interfaces.Core;
public interface ICartService
{
    Task<Result<Cart>> GetUserCartAsync();

    Task<Result<Cart>> AddItemToCartAsync(int productId, int quantity);

    Task<Result<Cart>> UpdateCartItemQuantityAsync(int productId, int newQuantity);

    Task<Result<Cart>> RemoveItemFromCartAsync(int productId);

    Task<Result> ClearCartAsync();

    Task<Result<decimal>> CalculateCartTotalAsync();
    Task<Result<Order>> CreateOrderFromCartAsync();

}

