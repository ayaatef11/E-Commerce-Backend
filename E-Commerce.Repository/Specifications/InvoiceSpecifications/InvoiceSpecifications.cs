using E_Commerce.Core.Models.InvoiceModels;

namespace E_Commerce.Repository.Specifications.InvoiceSpecifications;
public class InvoiceSpecifications : BaseSpecification<Invoice>

{
    public string? Email { get; set; }
    public InvoiceSpecifications()
    {
        IncludesCriteria.Add(P => P.Order!);
        OrderByDesc = P => P.OrderDate;
    }
    public InvoiceSpecifications(int id)
    {
        WhereCriteria = P => P.Id == id;

        IncludesCriteria.Add(P => P.Order!);
    }
    public InvoiceSpecifications(string email)
    {
        Email = email;
        WhereCriteria = p => p.UserEmail == email;
    }
    public InvoiceSpecifications(InvoiceSpecificationsParams param)
    {
        WhereCriteria = P => P.Id == param.Id || P.OrderId == param.OrderId || P.OrderDate == param.OrderDate || P.UserEmail == param.UserName || P.TotalAmount == param.TotalAmount;

        //IncludesCriteria.Add(P => P.Order!);
        OrderByDesc = P => P.OrderDate;
    }
}


