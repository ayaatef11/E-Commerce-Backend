using E_Commerce.DTOS.Invoice.Responses;
using E_Commerce.Application.Interfaces.Core;
using E_Commerce.Core.Shared.Utilties.Identity;

namespace E_Commerce.Controllers;
[Route("api/[controller]")]
[ApiController]
public class InvoiceController(IMapper _mapper, IInvoiceService _invoiceService, UserManager<AppUser> _userManager) : ControllerBase
{

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("getAll")]
    public async Task<ActionResult<IReadOnlyList<InvoiceResponse>>> GetAllInvoices()
    {
        var result = await _invoiceService.GetAllInvoicesAsync();
        if (!result.IsSuccess)
        {
            return Problem(
                title: result.Error.Title,
                statusCode: result.Error.StatusCode
            );
        }
        var invoicesResponse = _mapper.Map<IReadOnlyList<InvoiceResponse>>(result.Value);

        return Ok(invoicesResponse);
    }
   
    [Authorize(Roles = $"{Roles.User} ")]
    [HttpGet("{id}")]
    public async Task<ActionResult<InvoiceResponse>> GetInvoiceById(int id)
    {

        var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
        if (currentUserEmail == null)
        {
            return Unauthorized("User not authenticated");
        }

        var result = await _invoiceService.Get_invoice_by_id(id);
        if (result.IsFailure)
        {
            return StatusCode(result.Error.StatusCode, result.Error);
        }

        if (!User.IsInRole("Admin") && result.Value.UserEmail != currentUserEmail)
        {
            return Forbid();
        }

        var invoiceResponse = _mapper.Map<InvoiceResponse>(result.Value);

        return Ok(invoiceResponse);
    }
    
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("Admin/{id}")]
    public async Task<ActionResult<InvoiceResponse>> AdminGetInvoiceById(int id)
    {
        var result = await _invoiceService.Get_invoice_by_id(id);
        if (result.IsFailure)
        {
            return StatusCode(result.Error.StatusCode, result.Error);
        }
        var invoiceResponse = _mapper.Map<InvoiceResponse>(result.Value);
        return Ok(invoiceResponse);
    }
    
    [Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
    [HttpPost("create/{orderId}")]
    public async Task<ActionResult<InvoiceResponse>> CreateInvoice(int orderId)
    {
        var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(currentUserEmail))
            return Unauthorized("User email not found");
        var result = await _invoiceService.Create_Invoice(orderId);
        if (result.IsFailure)
        {
            return StatusCode(result.Error.StatusCode, result.Error);
        }

        if (!User.IsInRole(Roles.Admin) && result.Value.UserEmail != currentUserEmail)
            return Forbid();

        var invoiceResponse = _mapper.Map<InvoiceResponse>(result.Value);

        return Ok(invoiceResponse);
    }

    [Authorize(Roles = $"{Roles.User}")]
    [HttpGet("download/{invoiceId}")]
    public async Task<IActionResult> DownloadInvoice(int invoiceId)
    {

        var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(currentUserEmail))
        {
            return Unauthorized("User not authenticated");
        }
        var invoiceResult = await _invoiceService.Get_invoice_by_id(invoiceId);
        if (invoiceResult.IsFailure)
        {
            return StatusCode(invoiceResult.Error.StatusCode, invoiceResult.Error);
        }
        if (!User.IsInRole(Roles.Admin) && invoiceResult.Value.UserEmail != currentUserEmail)
        {
            return Forbid();
        }
        var fileResult = await _invoiceService.Download_Invoice(invoiceId);
        return fileResult.IsSuccess
            ? fileResult.Value
            : StatusCode(fileResult.Error.StatusCode, fileResult.Error);
    }
  
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("AdminDownload/{invoiceId}")]
    public async Task<IActionResult> AdminDownloadInvoice(int invoiceId, string currentUserEmail)
    {

        var invoiceResult = await _invoiceService.Get_invoice_by_id(invoiceId);
        if (invoiceResult.IsFailure)
        {
            return StatusCode(invoiceResult.Error.StatusCode, invoiceResult.Error);
        }
        if (!User.IsInRole(Roles.Admin) && invoiceResult.Value.UserEmail != currentUserEmail)
        {
            return Forbid();
        }
        var fileResult = await _invoiceService.Download_Invoice(invoiceId);
        return fileResult.IsSuccess
            ? fileResult.Value
            : StatusCode(fileResult.Error.StatusCode, fileResult.Error);
    }

    //[Authorize(Roles = Roles.Admin)]
    [HttpPost("send/{invoiceId}")]
    public async Task<IActionResult> SendInvoice(int invoiceId)
    {
        var result = await _invoiceService.Send_Invoice(invoiceId);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return StatusCode(result.Error.StatusCode, result.Error);
    }
   
    [Authorize(Roles = Roles.User)]
    [HttpGet("userInvoices")]
    public async Task<ActionResult<IReadOnlyList<InvoiceResponse>>> GetUserInvoices()
    {
        var result = await _invoiceService.Get_CurrentUserInvoices();
        if (result.IsFailure)
        {
            return NotFound();
        }
        var invoiceResponse = _mapper.Map<IReadOnlyList<InvoiceResponse>>(result.Value);
        return Ok(invoiceResponse);
    }
   
    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("delete/{invoiceId}")]
    public async Task<IActionResult> DeleteInvoice(int invoiceId)
    {

        var result = await _invoiceService.Delete_Invoice(invoiceId);
        return result.IsSuccess
            ? Ok("Invoice Deleted Successfully")
            : StatusCode(result.Error.StatusCode, result.Error);
    }
    
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin-get-user-invoices")]
    public async Task<ActionResult<IReadOnlyList<InvoiceResponse>>> AdminGetUserInvoices(string userEmail)
    {
        var result = await _invoiceService.Get_Invoices(userEmail);
        if (result.IsSuccess)
        {
            var invoiceResponse = _mapper.Map<IReadOnlyList<InvoiceResponse>>(result.Value);
            return Ok(invoiceResponse);
        }
        return Problem(
       title: result.Error.Title,
       detail: result.Error.Message,
       statusCode: result.Error.StatusCode
   );
    }

    //[Authorize(Roles = $"{Roles.Admin}")]
    //[HttpGet("sorted")]
    //public async Task<ActionResult<IReadOnlyList<InvoiceResponse>>> GetInvoicesSortedByDate([FromQuery] int page, int pageSize = 5, bool descending = true)
    //{
    //    var result = await _invoiceService.GetSortedInvoicesPagedAsync(page,pageSize,descending);

    //    if (result.IsFailure)
    //    {
    //        return Problem(
    //            title: result.Error.Title,
    //            statusCode: result.Error.StatusCode
    //        );
    //    }

    //    return Ok(_mapper.Map<IReadOnlyList<InvoiceResponse>>(result.Value));
    //}

    [Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
    [HttpGet("sorted")]
    public async Task<ActionResult<IReadOnlyList<InvoiceResponse>>> GetInvoicesSortedByDate([FromQuery] int page,[FromQuery] int pageSize = 5,[FromQuery] bool descending = true)
    {
        string buyerEmail = null;
        if (!User.IsInRole(Roles.Admin))
        {
            buyerEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(buyerEmail))
            {
                return Unauthorized();
            }
        }

        var result = await _invoiceService.GetSortedInvoicesPagedAsync(page,pageSize,descending,buyerEmail);
        if (result.IsFailure)
        {
            return Problem(
                title: result.Error.Title,
                statusCode: result.Error.StatusCode
            );
        }
        return Ok(_mapper.Map<IReadOnlyList<InvoiceResponse>>(result.Value));
    }
}


