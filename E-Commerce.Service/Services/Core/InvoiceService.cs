using E_Commerce.Core.Data; 
using QuestPDF.Infrastructure;

namespace E_Commerce.Application.Services.Core;
public class InvoiceService(IUnitOfWork _unitOfWork,UserManager<AppUser> _userManager,StoreContext _context,IHttpContextAccessor _httpContextAccessor,IEmailSenderService _emailService,InvoiceTemplateResolver _invoiceTemplate) : IInvoiceService
{
    private readonly string? _userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public async Task<Result<Invoice>> Create_Invoice(int orderId)//check later
    {
        try
        {
            if (orderId <= 0)
            {
                return Result.Failure<Invoice>(new Error
                {
                    Title = "order id invalid",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            //var order =await _context.Orders.Include(o => o.Items).ThenInclude(p => p.Product).FirstOrDefaultAsync(i=>i.Id==orderId);
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order == null)
                return Result.Failure<Invoice>(new Error 
                { Title = "Order not found",
                    StatusCode = 404
                });
            if (order.Status == OrderStatus.Completed)
            {
                return Result.Failure<Invoice>(new Error
                {
                    Title = "Order already completed",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            var user = await _userManager.FindByEmailAsync(order.BuyerEmail);
            if (user == null)
                return Result.Failure<Invoice>(Error.UserNotFound);
            order.Status = OrderStatus.Completed;

   
            var invoice = new Invoice
            {
                InvoiceNumber = $"INV-{order.Id}-{DateTime.UtcNow.Ticks}",
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.Price,
                CreatedAt = DateTime.UtcNow,
                UserEmail = order.BuyerEmail,
                IsPaid = false,
                UserName = order.BuyerName,
                UserPhoneNumber = order.BuyerPhoneNumber,
                UserAddress= order.BuyerAddress,
                PaymentDate = DateTime.UtcNow,
                InvoiceStatus = Status.Pending
            };

            await _unitOfWork.Repository<Invoice>().AddAsync(invoice);
            await _context.SaveChangesAsync();
            await _unitOfWork.CompleteAsync();
            return Result.Success(invoice);
        }
        catch (Exception ex)
        {
            return Result.Failure<Invoice>(Error.DatabaseError(ex.Message));
        }
    }

    public async Task<Result<bool>> Delete_Invoice(int invoiceId)
    {
        try
        {
            if (invoiceId <= 0)
            {
                return Result.Failure<bool>(new Error
                {
                    Title = "invalid invoice id ",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }
            var result = await _unitOfWork.Repository<Invoice>().DeleteById(invoiceId);
            if (!result)
                return Result.Failure<bool>(new Error { Title = "Invoice not found", StatusCode = 404 });

            await _unitOfWork.CompleteAsync();
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(Error.DatabaseError(ex.Message));
        }
    }

    public async Task<Result<FileResult>> Download_Invoice(int invoiceId)
    {
        try
        {
            if (invoiceId <= 0) {
                return Result.Failure<FileResult>(new Error 
                { Title = "invalid invoice id",
                    StatusCode = StatusCodes.Status400BadRequest
                });

            }
            var invoice = await _unitOfWork.Repository<Invoice>().GetByIdAsync(invoiceId);
            if (invoice == null)
                return Result.Failure<FileResult>(new Error { Title = "Invoice not found", StatusCode = 404 });

            var pdfBytes = CreateInvoicePdf(invoice);
            return Result.Success<FileResult>(new FileContentResult(pdfBytes, "application/pdf")
            {
                FileDownloadName = $"Invoice_{invoice.InvoiceNumber}.pdf"
            });
        }
        catch (Exception ex)
        {
            return Result.Failure<FileResult>(new Error
            {
                Title = "PDF Generation Failed",
                Message = ex.Message,
                StatusCode = 500
            });
        }
    }

    private byte[] CreateInvoicePdf(Invoice invoice)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text($" Invoice Number: {invoice.Id}").FontSize(20).SemiBold().AlignCenter();
                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Text($"Order Date:\t\t\t{invoice.OrderDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}");
                    col.Item().Text($"User Name:\t\t\t{invoice.UserName}");
                    col.Item().Text($"User Email:\t\t\t{invoice.UserEmail}");
                    col.Item().Text($"User Phone Number:\t\t\t{invoice.UserPhoneNumber}");
                    col.Item().Text($"User Address:\t\t\t{invoice.UserAddress}");
                    col.Item().Text($"Total Cost:\t\t\t{invoice.TotalAmount.ToString("C", CultureInfo.CreateSpecificCulture("en-EG"))} ");
                    col.Item().Text($"Order Id:\t\t\t{invoice.OrderId}");
                    col.Item().Text($"Created At:\t\t\t{invoice.CreatedAt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}");
                    col.Item().Text($"Is Paid:\t\t\t{invoice.IsPaid}");
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<Result<IReadOnlyList<Invoice>>> Filter_Invoices(InvoiceSpecificationsParams param)
    {
        try
        {
            var spec = new InvoiceSpecifications(param);
            var invoices = await _unitOfWork.Repository<Invoice>().GetAllWithSpecAsync(spec);
            return Result.Success(invoices);
        }
        catch (Exception ex)
        {
            return Result.Failure<IReadOnlyList<Invoice>>(Error.DatabaseError(ex.Message));
        }
    }

    public async Task<Result<IReadOnlyList<Invoice>>> Get_CurrentUserInvoices()
    {
        return await Get_Invoices(_userId);
    }
    
    public async Task<Result<string>> Generate_Invoice_Number_Automatically()
    {
        try
        {
            var lastInvoice = await _unitOfWork.Repository<Invoice>().GetLastOrDefaultAsync();
            int newNumber = 1;

            if (lastInvoice?.InvoiceNumber != null)
            {
                var parts = lastInvoice.InvoiceNumber.Split('-');
                if (parts.Length >= 2 && int.TryParse(parts[1], out int parsedNumber))
                {
                    newNumber = parsedNumber + 1;
                }
            }

            return Result.Success<string>($"INV-{newNumber.ToString("D4")}");
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(Error.DatabaseError(ex.Message));
        }
    }
   
    public async Task<IReadOnlyList<Invoice>> GetInvoiceByIdUsingSpecAsync(int id)
    {
        if(id <= 0)
        {
            return null;
        }
        var invoiceRepo = _unitOfWork.Repository<Invoice>();
        var spec = new InvoiceSpecifications(id);
        var invoices = await invoiceRepo.GetAllWithSpecAsync(spec);
        return invoices;
    }
   
    public async Task<Result<List<Invoice>>> GetAllInvoicesAsync()
    {
        try
        {
            var invoices = await _context.Invoices
              .Include(i => i.Order)
              .ThenInclude(o => o.Items)
              .ThenInclude(oi => oi.Product).ToListAsync();
            return Result.Success(invoices);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Invoice>>(Error.DatabaseError(ex.Message));
        }
    }
  
    public async Task<Result<IReadOnlyList<Invoice>>> Get_Invoices(string email)
    {
        try
        {
           
            var invoices = await _context.Invoices
                .Where(i => i.UserEmail == email).Include(i=>i.Order).ThenInclude(o=>o.Items).ThenInclude(p=>p.Product) 
                .ToListAsync();

            return Result.Success<IReadOnlyList<Invoice>>(invoices);
        }
        catch (Exception ex)
        {
            return Result.Failure<IReadOnlyList<Invoice>>(Error.DatabaseError(ex.Message));
        }
    }
    public async Task<Result<bool>> Pay_Invoice(int invoiceId, string paymentMethod)
    {
        try
        {
            if (invoiceId <= 0)
            {
                return Result.Failure<bool>(new Error { 
                    Title = "Invalid invoice id ",
                    StatusCode = StatusCodes.Status400BadRequest }
                );

            }
            var invoice = await _unitOfWork.Repository<Invoice>().GetByIdAsync(invoiceId);
            if (invoice == null)
                return Result.Failure<bool>(new Error { Title = "Invoice not found", StatusCode = 404 });

            if (invoice.IsPaid)
                return Result.Failure<bool>(new Error { Title = "Invoice already paid", StatusCode = 400 });

            invoice.IsPaid = true;
            invoice.PaymentDate = DateTimeOffset.Now;
            invoice.PaymentMethod = paymentMethod;
            _unitOfWork.Repository<Invoice>().Update(invoice);
            await _unitOfWork.CompleteAsync();
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(Error.DatabaseError(ex.Message));
        }
    }
   
    public async Task<Result<Invoice>> Send_Invoice(int invoiceId)
    {
        if (invoiceId <= 0)
        {
            return Result.Failure<Invoice>(new Error
            {
                Title = "invalid invoice id ",
                StatusCode = StatusCodes.Status400BadRequest,
            });
        }
        var invoice = await _context.Invoices
                        .Include(i => i.Order)
                        .ThenInclude(o => o.Items)
                        .ThenInclude(oi => oi.Product)
                        .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null)
            return Result.Failure<Invoice>(new Error
            {
                Title = "invoice not found ",
                StatusCode = StatusCodes.Status404NotFound,
            });
        var user = await _userManager.FindByEmailAsync(invoice.UserEmail);
        if (user == null)
            return Result.Failure<Invoice>(new Error
            {
                Title = "user  not found ",
                StatusCode = StatusCodes.Status400BadRequest,
            });

        string body =await _invoiceTemplate.ResolveInvoiceTemplate(invoice.UserName, invoice);

        await _emailService.SendEmailAsync(user.Email, "Your Invoice From Causmatic Store", body);
        invoice.InvoiceStatus = Status.Sent;
     await   _context.SaveChangesAsync();
      await  _unitOfWork.CompleteAsync();
        return Result.Success(invoice);
    }
    
    public async Task<Result<Invoice>> Get_invoice_by_id(int id)
    {
        try
        {
            if (id <= 0)
            {
                return Result.Failure<Invoice>(new Error
                {
                    Title = "Invalid id ",
                    StatusCode = StatusCodes.Status400BadRequest
                });

            }
            var invoice = await _context.Invoices
                .Include(i => i.Order)
                .ThenInclude(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(i => i.Id == id);
            return invoice == null
                ? Result.Failure<Invoice>(new Error { Title = "Invoice not found", StatusCode = 404 })
                : Result.Success(invoice);
        }
        catch (Exception ex)
        {
            return Result.Failure<Invoice>(Error.DatabaseError(ex.Message));
        }
    }
 
    public async Task<Result<string>> Track_Invoice_Status(int invoiceId)
    {
        if (invoiceId <= 0)
        {
            return Result.Failure<string>(new Error
            {
                Title = "Invalid id ",
                StatusCode = StatusCodes.Status400BadRequest
            });

        }
        var invoiceResult = await Get_invoice_by_id(invoiceId);
        if (invoiceResult.IsFailure)
            return Result.Failure<string>(invoiceResult.Error);
        var status = $"حالة الفاتورة رقم {invoiceResult.Value.Id}: " +
                    $"{(invoiceResult.Value.IsPaid ? "مدفوعة" : invoiceResult.Value.InvoiceStatus)}";

        return Result.Success<string>(status);
    }

    public async Task<Result> Update_Invoice(Invoice invoice)
    {
        try
        {
            _unitOfWork.Repository<Invoice>().Update(invoice);
            await _unitOfWork.CompleteAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.DatabaseError(ex.Message));
        }
    }

    public async Task<Result<IReadOnlyList<Invoice>>> GetPendingInvoicesAsync()
    {//check status is beding not sent
        try
        {
            var spec = new InvoiceSpecifications();
            var invoices = await _unitOfWork.Repository<Invoice>().GetAllWithSpecAsync(spec);
            return Result.Success(invoices);
        }
        catch (Exception ex)
        {
            return Result.Failure<IReadOnlyList<Invoice>>(Error.DatabaseError(ex.Message));
        }
    }

    public async Task SendInvoiceAsync(Invoice invoice)
    {
        var pdfBytes = CreateInvoicePdf(invoice);
        await _emailService.SendEmailAsync(
            toEmail: invoice.UserEmail,
            subject: $"فاتورة #{invoice.Id}",
            body: "يرجى الاطلاع على المرفقات"
            //attachments: new[] { new Attachment(pdfBytes, $"Invoice_{invoice.Id}.pdf") }
        );
    }
    public async Task MarkAsSentAsync(int invoiceId)
    {
        var invoice=await Get_invoice_by_id(invoiceId);
        invoice.Value.InvoiceStatus = Status.Sent;
        _unitOfWork.Repository<Invoice>().Update(invoice.Value);
    }

    public async Task<Result<IReadOnlyList<Invoice>>> GetInvoicesSortedByDateAsync(bool descending = true)
    {
        try
        {
            var query = _context.Invoices.Include(i => i.Order)
                .ThenInclude(o => o.Items)
                .ThenInclude(oi => oi.Product).AsQueryable();

            query = descending
                ? query.OrderByDescending(i => i.CreatedAt)
                : query.OrderBy(i => i.CreatedAt);

            var invoices = await query.ToListAsync();
            return Result.Success<IReadOnlyList<Invoice>>(invoices);
        }
        catch (Exception ex)
        {
            return Result.Failure<IReadOnlyList<Invoice>>(new Error(
                "Database.Error",
                ex.Message,
                StatusCodes.Status500InternalServerError));
        }
    }

    public async Task<Result<List<Invoice>>> GetSortedInvoicesPagedAsync(int page,int pageSize = 5,bool descending = true,string buyerEmail = null)  
    {
        try
        {
            var query = _context.Invoices.Include(i => i.Order)
                .ThenInclude(o => o.Items)
                .ThenInclude(oi => oi.Product).AsQueryable();
            if (!string.IsNullOrEmpty(buyerEmail))
            {
                query = query.Where(i => i.UserEmail == buyerEmail);
            }
            query = descending
                ? query.OrderByDescending(i => i.CreatedAt)
                : query.OrderBy(i => i.CreatedAt);

            var totalCount = await query.CountAsync();
            var invoices = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Result.Success(new List<Invoice>(invoices));
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Invoice>>(new Error(
                "Database.Error",
                ex.Message,
                StatusCodes.Status500InternalServerError));
        }
    }
}
