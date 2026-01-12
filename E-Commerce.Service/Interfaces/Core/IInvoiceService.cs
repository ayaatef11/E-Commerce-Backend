using E_Commerce.Core.Shared.Results;
using E_Commerce.Repository.Specifications.InvoiceSpecifications;

namespace E_Commerce.Application.Interfaces.Core;
    public interface IInvoiceService
    {
        Task<Result<Invoice>> Create_Invoice(int orderId);
        Task<Result<bool>> Delete_Invoice(int invoiceId);
        Task<Result<FileResult>> Download_Invoice(int invoiceId);
        Task<Result<IReadOnlyList<Invoice>>> Filter_Invoices(InvoiceSpecificationsParams param);
        Task<Result<string>> Generate_Invoice_Number_Automatically();
        Task<Result<List<Invoice>>> GetAllInvoicesAsync();
        Task<IReadOnlyList<Invoice>> GetInvoiceByIdUsingSpecAsync(int id);
        Task<Result<IReadOnlyList<Invoice>>> Get_Invoices(string userEmail);
        Task<Result<IReadOnlyList<Invoice>>> Get_CurrentUserInvoices();
        Task<Result<Invoice>> Send_Invoice(int invoiceId);
        Task<Result<Invoice>> Get_invoice_by_id(int id);
        Task<Result<bool>> Pay_Invoice(int invoiceId, string paymentMethod);
        Task<Result<string>> Track_Invoice_Status(int invoiceId);
        Task<Result> Update_Invoice(Invoice invoice);
        Task<Result<IReadOnlyList<Invoice>>> GetPendingInvoicesAsync();
        Task SendInvoiceAsync(Invoice invoice);
        Task MarkAsSentAsync(int invoiceId);
        Task<Result<IReadOnlyList<Invoice>>> GetInvoicesSortedByDateAsync(bool descending = true);
        Task<Result<List<Invoice>>> GetSortedInvoicesPagedAsync(int page, int pageSize = 5, bool descending = true, string buyerEmail=null);
}

