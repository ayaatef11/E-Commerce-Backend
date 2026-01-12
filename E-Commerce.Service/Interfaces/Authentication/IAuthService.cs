using E_Commerce.Application.Common.DTOS.Requests;
using E_Commerce.Application.Common.DTOS.Responses;
using E_Commerce.Core.Shared.Results;

namespace E_Commerce.Application.Interfaces.Authentication;
public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginDto model);
    Task<RegisterResult> RegisterUserAsync(RegisterDto model);
    Task Generate2FactorToken(AppUser user);
    Task<VerificationResult> SendEmailVerificationCodeAsync(string email);

    Task<UserResult> GetCurrentUserAsync(string userEmail);
    Task<Result> ConfirmEmailAsync(ConfirmEmailDto model);
    Task<Result<AuthResult>> AuthenticateAsync(LoginDto loginRequest);


    ChallengeResult InitiateGoogleLogin();

     Task<LoginResultDto> HandleGoogleResponse();

     Task<LoginResultDto> RegisterWithGoogle(GoogleRegisterDto dto);
    }