using E_Commerce.DTOS.Invoice.Responses;

namespace E_Commerce.Mapping.InvoiceMapping;
    public class InvoiceProfiler:Profile
    {
    public InvoiceProfiler()
    {

        CreateMap<Invoice, InvoiceResponse>()
          .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Order.Id))
          .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.Order.OrderDate))
          .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Order.Items));

    }
}

