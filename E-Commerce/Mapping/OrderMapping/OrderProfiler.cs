namespace E_Commerce.Mapping.OrderMapping;
public class OrderProfiler:Profile
    {
    public OrderProfiler()
    {
        CreateMap<CreateOrderRequest, Order>();

      
        CreateMap<Order, OrderResponse>();

    }
    }

