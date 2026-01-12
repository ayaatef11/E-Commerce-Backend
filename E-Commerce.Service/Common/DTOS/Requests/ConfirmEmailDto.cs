namespace E_Commerce.Application.Common.DTOS.Requests
{
    public class ConfirmEmailDto
    {
        public string Email { get; set; }=string.Empty;
        public string Code { get; set; }= string.Empty;
    }
}
