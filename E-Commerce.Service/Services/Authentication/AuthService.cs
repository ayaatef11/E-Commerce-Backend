using E_Commerce.Core.Models.AuthModels;
using E_Commerce.Application.Common.DTOS.Requests;
using E_Commerce.Application.Common.DTOS.Responses;
using E_Commerce.Application.Common.Resolvers;
using E_Commerce.Application.Interfaces.Authentication;
using E_Commerce.Core.Shared.Utilties.Identity;

namespace E_Commerce.Application.Services.Authentication;
public class AuthService(
        SignInManager<AppUser> _signInManager,
            UserManager<AppUser> _userManager,
            IJwtAuthService _jwtAuth,
            ITokenService _tokenService,
            ILogger<AuthService> _logger,
            IEmailSenderService _emailService, EmailCodes _emailCodes,
            TwoFactorTemplateResolver _twofactorResolver, IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor, IHttpContextAccessor _httpContextAccessor) : IAuthService
{

    IUrlHelper _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
    public async Task<RegisterResult> RegisterUserAsync(RegisterDto model)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return RegisterResult.Failure(new[] { "Email is already registered" });
            }

            var newUser = new AppUser
            {
                Email = model.Email,
                Full_Name = model.FullName,
                UserName = model.Email.Split('@')[0]
            };

            var creationResult = await _userManager.CreateAsync(newUser, model.Password);
            if (!creationResult.Succeeded)
            {
                return RegisterResult.Failure(creationResult.Errors.Select(e => e.Description));
            }

            var confirmationCode = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            newUser.EmailConfirmationCode = confirmationCode;
            newUser.EmailConfirmationCodeExpiry = DateTime.UtcNow.AddHours(24);
            await _userManager.UpdateAsync(newUser);

            if (model.IsTwoFactorEnabled)
            {
                await _userManager.SetTwoFactorEnabledAsync(newUser, true);
            }

            try
            {
                await _emailCodes.SendEmailConfirmationCode(
                    newUser.Email,
                    confirmationCode,
                    newUser.Full_Name
                );
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, "Failed to send confirmation email to {Email}", newUser.Email);
            }


            await _userManager.AddToRoleAsync(newUser, Roles.User);

            return RegisterResult.Success(newUser.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return RegisterResult.Failure(new[] { "An error occurred during registration" });
        }
    }


    public async Task<VerificationResult> SendEmailVerificationCodeAsync(string email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return VerificationResult.Failure("User not found");
            }

            var confirmationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            user.EmailConfirmationCode = confirmationCode;
            user.EmailConfirmationCodeExpiry = DateTime.UtcNow.AddHours(24);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return VerificationResult.Failure("Failed to generate verification code");
            }

            await _emailCodes.SendEmailConfirmationCode(
                user.Email,
                confirmationCode,
                user.Full_Name
            );

            return VerificationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification code");
            return VerificationResult.Failure("Failed to send verification email");
        }
    }

    public async Task<AuthResult> LoginAsync(LoginDto model)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning($"Login attempt for non-existent email: {model.Email}");
                return AuthResult.Failure("Invalid credentials");
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning($"Login attempt for unconfirmed email: {model.Email}");
                return AuthResult.Failure("Please confirm your email before logging in");
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning($"Locked out account login attempt: {model.Email}");
                return AuthResult.Failure("Account locked due to multiple failed attempts. Please try again later or reset your password.");
            }

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                await _userManager.AccessFailedAsync(user);
                _logger.LogWarning($"Failed login attempt for: {model.Email}");
                return AuthResult.Failure("Invalid credentials");
            }

            await _userManager.ResetAccessFailedCountAsync(user);
            var tokenResult = await _tokenService.GenerateJwtToken(user);

            // Set refresh token in HTTP-only cookie
            _httpContextAccessor.HttpContext!.Response.Cookies.Append(
                "refreshToken",
                tokenResult.RefreshToken!,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                });


            _logger.LogInformation($"Successful login for: {model.Email}");

            return AuthResult.Success(
                token: tokenResult.Token,
                userId: user.Id,
                email: user.Email,
                username: user.UserName
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during login for: {model.Email}");
            return AuthResult.Failure("An error occurred during login");
        }
    }

    public async Task Generate2FactorToken(AppUser user)
    {
        var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
        if (!providers.Contains("Email"))
        {
            return;

        }
        string token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
        string message = await _twofactorResolver.ResolveCodesTwoFactorAsync(token, user.Full_Name);
        ;
        if (user.Email == null) return;
        await _emailService.SendEmailAsync(user.Email, "Two Factor Authentication", message);

    }

    public async Task<UserResult> GetCurrentUserAsync(string userEmail)
    {
        try
        {
            if (string.IsNullOrEmpty(userEmail))
            {
                return UserResult.Failure("User not authenticated");
            }

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                _logger.LogWarning($"User not found for email: {userEmail}");
                return UserResult.Failure("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = await _jwtAuth.GenerateJwtToken(user); 

           return  new UserResult
           {
               success = true,
               UserId = user.Id,
               Email = user.Email,
               UserName = user.UserName,
               Roles = roles,
               Token = token,
               Full_Name = user.Full_Name,
               Address = user.Address,
               Job_Title = user.Job_Title,
               PhotoPath = user.PhotoPath,
               PhoneNumber=user.PhoneNumber
           };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving user: {userEmail}");
            return UserResult.Failure("Error retrieving user information");
        }
    }

    public async Task<Result> ConfirmEmailAsync(ConfirmEmailDto model)
    {
        try
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Code))
            {
                _logger.LogWarning("Invalid email confirmation request");
                return Result.Failure(new Error(
                    "Confirmation.InvalidRequest",
                    "Invalid email confirmation request",
                    StatusCodes.Status400BadRequest
                ));
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning($"User not found for email: {model.Email}");
                return Result.Failure(new Error(
                    "User.NotFound",
                    "User not found",
                    StatusCodes.Status404NotFound
                ));
            }

            var isValidToken = await _userManager.VerifyUserTokenAsync(
                user,
                "emailconfirmation",
                "EmailConfirmation",
                model.Code
            );

            if (!isValidToken)
            {
                _logger.LogWarning($"Invalid confirmation code for user: {user.Id}");
                return Result.Failure(new Error(
                    "Confirmation.InvalidCode",
                    "Invalid confirmation code",
                    StatusCodes.Status400BadRequest
                ));
            }

            user.EmailConfirmed = true;
            user.EmailConfirmationCode = "0";

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _logger.LogError($"Failed to update user: {string.Join(", ", updateResult.Errors)}");
                return Result.Failure(Error.DatabaseError("Failed to confirm email"));
            }

            _logger.LogInformation($"Email confirmed for user: {user.Id}");
            return Result.Success("Email confirmed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email");
            return Result.Failure(Error.DatabaseError("Error confirming email"));
        }
    }
    public async Task<Result<AuthResult>> AuthenticateAsync(LoginDto loginRequest)
    {
        try
        {
            if (loginRequest == null ||
                string.IsNullOrEmpty(loginRequest.Email) ||
                string.IsNullOrEmpty(loginRequest.Password))
            {
                return Result.Failure<AuthResult>(
                    new Error("Auth.InvalidRequest", "Invalid request payload", 400));
            }

            var user = await _userManager.FindByEmailAsync(loginRequest.Email);
            if (user == null)
            {
                return Result.Failure<AuthResult>(
                    new Error("Auth.InvalidCredentials", "Invalid authentication attempt", 401));
            }

            if (!await _userManager.CheckPasswordAsync(user, loginRequest.Password))
            {
                return Result.Failure<AuthResult>(
                    new Error("Auth.InvalidCredentials", "Invalid authentication attempt", 401));
            }

            if (await _userManager.GetTwoFactorEnabledAsync(user))
            {
                await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = await _jwtAuth.GenerateJwtToken(user);

            return Result.Success(new AuthResult
            {
                IsAuthSuccessful = true,
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                Roles = roles.ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication error for email: {Email}", loginRequest?.Email);
            return Result.Failure<AuthResult>(
                Error.DatabaseError("An error occurred during authentication"));
        }
    }
    public async Task<Result<AuthResult>> VerifyTwoFactorAsync(TwoFactorDto twoFactorDto)
    {
        try
        {
            if (twoFactorDto == null ||
                string.IsNullOrEmpty(twoFactorDto.Email) ||
                string.IsNullOrEmpty(twoFactorDto.Token))
            {
                return Result.Failure<AuthResult>(
                    new Error("2FA.InvalidRequest", "Invalid request data", 400));
            }

            var user = await _userManager.FindByEmailAsync(twoFactorDto.Email);
            if (user == null)
            {
                _logger.LogWarning($"2FA attempt for non-existent user: {twoFactorDto.Email}");
                return Result.Failure<AuthResult>(
                    new Error("2FA.InvalidUser", "Invalid request", 400));
            }

            var isValidToken = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                twoFactorDto.Provider,
                twoFactorDto.Token);

            if (!isValidToken)
            {
                _logger.LogWarning($"Invalid 2FA token for user: {user.Id}");
                return Result.Failure<AuthResult>(
                    new Error("2FA.InvalidToken", "Invalid token", 401));
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            var roles = await _userManager.GetRolesAsync(user);
            var token = await _jwtAuth.GenerateJwtToken(user);

            return Result.Success(new AuthResult
            {
                IsAuthSuccessful = true,
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                Roles = roles.ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during 2FA verification");
            return Result.Failure<AuthResult>(
                Error.DatabaseError("Error processing two-factor authentication"));
        }
    }


    public ChallengeResult InitiateGoogleLogin()
    {
        var redirectUrl = _urlHelper.Action("GoogleResponse", "Auth",
            values: null,
            protocol: _httpContextAccessor.HttpContext.Request.Scheme);

        var properties = _signInManager.ConfigureExternalAuthenticationProperties(
            "Google",
            redirectUrl);

        return new ChallengeResult("Google", properties);
    }

    public async Task<LoginResultDto> HandleGoogleResponse()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
            return new LoginResultDto { Error = "Failed to retrieve Google login information" };

        if (string.IsNullOrEmpty(info.ProviderKey))
            return new LoginResultDto { Error = "Invalid Google provider data" };

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
            return new LoginResultDto { Error = "Email claim not received from Google" };

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            // Check for existing linked login
            var logins = await _userManager.GetLoginsAsync(existingUser);
            var isLinked = logins.Any(l => l.LoginProvider == "Google" && l.ProviderKey == info.ProviderKey);

            if (!isLinked)
            {
                var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                if (!addLoginResult.Succeeded)
                    return new LoginResultDto { Error = "Failed to link Google account" };
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false);

            if (!signInResult.Succeeded)
                return new LoginResultDto { Error = "Google authentication failed" };


            var token = await _jwtAuth.GenerateJwtToken(existingUser);
            return new LoginResultDto { Success = false, Token = token };
        }

        return new LoginResultDto
        {
            RequiresRegistration = true,
            Success = false,
            Error = "Complete registration",
            Token = email
        };
    }

    public async Task<LoginResultDto> RegisterWithGoogle(GoogleRegisterDto dto)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
            return new LoginResultDto { Error = "Google session expired" };

        var user = new AppUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            Full_Name = dto.FullName,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
            return new LoginResultDto { Error = string.Join(", ", createResult.Errors.Select(e => e.Description)) };

        await _userManager.AddLoginAsync(user, info);
        await _signInManager.SignInAsync(user, isPersistent: false);

        var token = await _jwtAuth.GenerateJwtToken(user);
        return new LoginResultDto { Success = false, Token = token };
    }

}
 



