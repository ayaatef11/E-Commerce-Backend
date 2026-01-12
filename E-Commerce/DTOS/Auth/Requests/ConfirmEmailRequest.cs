namespace E_Commerce.DTOS.Auth.Requests
{
    public class ConfirmEmailRequest
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
    }
}
