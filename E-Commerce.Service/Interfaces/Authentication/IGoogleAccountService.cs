using E_Commerce.Application.Common.DTOS.Responses;
using E_Commerce.Core.Shared.Settings;


namespace E_Commerce.Application.Interfaces.Authentication
{
    public interface IGoogleAccountService
    {

         Task<OAuthGoogleTokenResponse?> GetGoogleAccessTokenAsync(string authorizationCode, string clientId, string clientSecret, string baseUrl);

          Task<GoogleUserInformation?> GetGoogleUserInfoAsync(string accessToken);
         string RegisterEmailBody(string verificationUrl, string userName);

         string ResetPasswordEmailBody(string verificationUrl, string userName);

         string GenerateSecureCode();
         string HashCode(string code);
         bool ConstantComparison(string a, string b);
    }
}
