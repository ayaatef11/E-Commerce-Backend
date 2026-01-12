namespace E_Commerce.DTOS.Auth.Requests
{
    public class TwoFactorRequest
    {
        public string Email { get; set; } = null!;
        public string Provider { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
