using E_Commerce.Core.Shared.Results;

namespace E_Commerce.Application.Interfaces.Core;
public interface IOrderService
{
    Task<Result<Order>> CreateOrder(Order order);
    Task<Result<Order>> GetById(int id, string BuyerEmail);
    Task<Result<Order>> GetById(int id);

    Task<Result<Order>> AddProductToOrder(int orderId, OrderItem itemDto);

    Task<Result<Order>> UpdateProductInOrder(int orderId, int productId, int newQuantity);

    Task<Result<Order>> RemoveProductFromOrder(int orderId, int productId);

    Task<string> TrackOrderStatus(int orderId);

    Task<Result<Order>> UpdateOrderStatus(int orderId, string newStatus);

    Task<Result<Order>> CancelOrder(int orderId, string email);
    Task<Result<List<Order>>> GetOrdersSortedByDateAsync(int page, int pageSize = 5, bool descending = true, string? buyerEmail = null);

}

