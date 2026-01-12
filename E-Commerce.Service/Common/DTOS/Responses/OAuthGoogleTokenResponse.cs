namespace E_Commerce.Application.Common.DTOS.Responses;
public sealed record OAuthGoogleTokenResponse
{
    public string Access_Token { get; set; } = null!;
}

