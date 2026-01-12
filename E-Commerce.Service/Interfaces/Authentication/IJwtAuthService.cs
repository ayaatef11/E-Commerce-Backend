namespace E_Commerce.Application.Interfaces.Authentication;
public interface IJwtAuthService
{
    Task<string> GenerateJwtToken(AppUser user);
    ClaimsPrincipal ValidateJwtToken(string token);
    TokenValidationParameters GetValidationParameters();
}