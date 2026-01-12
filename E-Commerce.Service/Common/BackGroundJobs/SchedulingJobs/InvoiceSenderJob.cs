using E_Commerce.Application.Interfaces.Core;
using Quartz;
namespace E_Commerce.Application.Common.BackGroundJobs.SchedulingJobs;
public class InvoiceSenderJob(IInvoiceService _invoiceService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var pendingInvoices = await _invoiceService.GetPendingInvoicesAsync();

            foreach (var invoice in pendingInvoices.Value)
            {
                await _invoiceService.SendInvoiceAsync(invoice);

                await _invoiceService.MarkAsSentAsync(invoice.Id);
            }
        }
        catch (Exception ex) {
            throw new JobExecutionException(ex, true);
        }
        
        }
}