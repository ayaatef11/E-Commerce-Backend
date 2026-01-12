namespace E_Commerce.Application.Common.DTOS.Requests;
public class TokenRequest
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}

