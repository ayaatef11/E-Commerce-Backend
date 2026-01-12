using E_Commerce.Core.Models.CartModels;
using E_Commerce.DTOS.Cart.Responses;

namespace E_Commerce.Mapping.CartMapping;
    public class CartProfiler:Profile
    {
    public CartProfiler()
    {
        CreateMap<Cart, CartResponse>();

    }
}

