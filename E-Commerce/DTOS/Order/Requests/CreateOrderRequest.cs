using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOS.Order.Requests;

public class CreateOrderRequest
{
    [Required, EmailAddress]
    public string BuyerEmail { get; set; } = string.Empty;


    [MinLength(1)]
    public ICollection<OrderItemRequest> Items { get; set; } = new HashSet<OrderItemRequest>();

}