using E_Commerce.DTOS.Invoice.Responses;

namespace E_Commerce.Mapping.InvoiceMapping;
    public class InvoiceItemProfiler : Profile
{
    public InvoiceItemProfiler()
    {
        
        CreateMap<OrderItem, InvoiceItemResponse>()
        .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));

 }
}

