using E_Commerce.Core.Models.CartModels;
using E_Commerce.DTOS.Cart.Responses;

namespace E_Commerce.Mapping.CartMapping;
    public class CartItemProfiler:Profile
    {
    public CartItemProfiler()
    {
        CreateMap<CartItem, CartItemResponse>()
             .ForMember(dest => dest.PictureUrl,
                       opt => opt.MapFrom(src => src.Product.PictureUrl));
    }
}

