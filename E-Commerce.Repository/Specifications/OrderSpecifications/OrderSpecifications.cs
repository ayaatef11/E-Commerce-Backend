using E_Commerce.Core.Models.OrderModels;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Repository.Specifications.OrderSpecifications;
    public class OrderSpecifications : BaseSpecification<Order>
    {
    public string? Email { get; }  
    public int OrderId { get; }  
    public OrderSpecifications(string buyerEmail)
        {
            WhereCriteria = P => P.BuyerEmail == buyerEmail;
            IncludesCriteria.Add(P => P.Items);

            OrderByDesc = P => P.OrderDate;
        }
    public OrderSpecifications(string buyerEmail, int orderId, bool includeProducts = false)//:base(x=>x.Id==id)
        {
        Email = buyerEmail;
        OrderId = orderId;
        WhereCriteria = P => P.BuyerEmail == buyerEmail && P.Id == orderId;
        IncludesCriteria.Add(P => P.Items);


        if (includeProducts)
        {
            //NestedIncludes.Add(query => query.Include(x => x.Items).ThenInclude(ps => ps.Product));

        } 
    }
    public OrderSpecifications(int orderId, bool includeProducts = false)
    {
        OrderId = orderId;
        WhereCriteria = P => P.Id == orderId;

        IncludesCriteria.Add(P => P.Items);
        if (includeProducts)
        {
            AddNestedInclude(query => query.Include(x => x.Items).ThenInclude(ps => ps.Product));

        } 
    }
}
