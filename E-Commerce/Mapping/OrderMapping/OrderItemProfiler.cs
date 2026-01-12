namespace E_Commerce.Mapping.OrderMapping
{
    public class OrderItemProfiler:Profile
    {
        public OrderItemProfiler()
        {
            CreateMap<OrderItemRequest, OrderItem>()
       .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
       .ForPath(dest => dest.Product.Name, opt => opt.MapFrom(src => src.ProductName));
                CreateMap<OrderItem, OrderItemResponse>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                    .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                    .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                    .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total));
            }
        }
    }
