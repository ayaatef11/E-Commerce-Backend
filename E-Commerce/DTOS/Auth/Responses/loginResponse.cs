namespace E_Commerce.DTOS.Auth.Responses
{
    public class loginResponse
    {
        public string UserName { get; set; }
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public string? RefreshToken { get; set; }
    }
}
