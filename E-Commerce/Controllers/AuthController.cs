using E_Commerce.Application.Common.DTOS.Requests;
using E_Commerce.Application.Common.DTOS.Responses;
using E_Commerce.Application.Interfaces.Authentication;
using E_Commerce.Core.Shared.Utilties.Identity;
using Google.Apis.Auth;
using Microsoft.IdentityModel.JsonWebTokens;
namespace Controllers;
[Route("api/[controller]")]
[ApiController]
public class AuthController(ITokenService _tokenService,IAuthService _authService, UserManager<AppUser> _userManager, IJwtAuthService _jwtAuth,IMapper _mapper ) : ControllerBase
{
    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Errors = ModelState.Values.SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage)
            });
        }

        if (model.Password != model.ConfirmPassword)
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Errors = new[] { "Password and confirmation password do not match" }
            });
        }

        var modelRequest = _mapper.Map<RegisterDto>(model);
        var result = await _authService.RegisterUserAsync(modelRequest);

        return result.success
            ? Ok(new
            {
                Success = true,
                Message = "Registration successful. Please check your email for confirmation code.",
                result.UserEmail
            })
            : BadRequest(new AuthResponse
            {
                Success = false,
                Errors = result.Errors
            });
    }
     
        [HttpPost("send-verification-code")] 
    public async Task<IActionResult> SendVerificationCode([FromBody] SendVerificationCodeRequest request)
        {
            var result = await _authService.SendEmailVerificationCodeAsync(request.Email);

            return result.Succeeded
                ? Ok(new { Message = "Verification code sent successfully" })
                : BadRequest(new AuthResponse
                {
                    Success = false
                });
    }

   
    [HttpPost("ConfirmEmail")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest model)
    {
        var modelRequest = _mapper.Map<ConfirmEmailDto>(model);
        var result = await _authService.ConfirmEmailAsync(modelRequest);

        return result.IsSuccess
            ? Ok(new { Success = true, result.Message })
            : StatusCode(result.Error.StatusCode, result.Error);
    }


    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest modelRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var model = _mapper.Map<LoginDto>(modelRequest);
        var result = await _authService.LoginAsync(model);

        if (!result.success)
        {
            return Unauthorized(new AuthResponse
            {
                Success = false,
                Errors = result.Errors
            });
        }
   
        return Ok(new AuthResponse(result));
    }

    /*   [HttpGet("google-login")]
       public IActionResult GoogleLogin()
       {
           var redirectUrl = Url.Action("GoogleResponse", "Auth");
           var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
           return Challenge(properties, "Google");
       }

       [HttpGet("google-response")]
       public async Task<IActionResult> GoogleResponse()
       {
           var info = await _signInManager.GetExternalLoginInfoAsync();
           if (info == null)
               return BadRequest("Error loading external login information.");

           var email = info.Principal.FindFirstValue(ClaimTypes.Email);
           var name = info.Principal.FindFirstValue(ClaimTypes.Name);

           if (string.IsNullOrEmpty(email))
               return BadRequest("Email claim is missing.");

           var user = await _userManager.FindByEmailAsync(email);

           if (user == null)
           {
               user = new AppUser
               {
                   UserName = email.Split("@")[0],
                   Email = email,
                   Full_Name = name,
                   EmailConfirmed = true
               };

               var createResult = await _userManager.CreateAsync(user);
               if (!createResult.Succeeded)
                   return BadRequest(createResult.Errors);
               var roleResult = await _userManager.AddToRoleAsync(user, Roles.User);
               if (!roleResult.Succeeded)
                   return BadRequest(roleResult.Errors);
           }

           var existingLogins = await _userManager.GetLoginsAsync(user);
           if (!existingLogins.Any(l => l.LoginProvider == "Google"))
           {
               var addLoginResult = await _userManager.AddLoginAsync(user, info);
               if (!addLoginResult.Succeeded)
                   return BadRequest(addLoginResult.Errors);
           }
           var signInResult = await _signInManager.ExternalLoginSignInAsync("Google", info.ProviderKey, isPersistent: false);
           if (!signInResult.Succeeded)
               return BadRequest("Failed to sign in.");

           var token = _jwtAuth.GenerateJwtToken(user);
           return Ok(new { Token = token });
       }

   */


    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        // 1. Validate Google token
        var payload = await ValidateGoogleToken(request.IdToken);
        if (payload == null)
            return BadRequest("Invalid Google token");

        var user = await _userManager.FindByEmailAsync(payload.Email);
        if (user == null)
        {
            user = new AppUser
            {
                UserName = payload.Email.Split("@")[0],
                Email = payload.Email,
                Full_Name = payload.Name,
                EmailConfirmed = true
            };


            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                return BadRequest(createResult.Errors);

            await _userManager.AddToRoleAsync(user, Roles.User);
        }
        string token = await _jwtAuth.GenerateJwtToken(user);

        return Ok(new { Token = token });
    }
    private async Task<GoogleJsonWebSignature.Payload> ValidateGoogleToken(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new[] { /*configuration["Google:ClientId"] */"663678654421-v7k61u0sie7jql2bt3co7ebm8savo688.apps.googleusercontent.com" }
            };

            return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
        catch
        {
            return null;
        }
    }

    public class GoogleLoginRequest
    {
        public string IdToken { get; set; }
    }


    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _tokenService.RefreshTokenAsync(request.Token, request.RefreshToken);

        if (!result.success)
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Errors = result.Errors
            });
        }

        return Ok(new AuthResponse(result));
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
    {
        var result = await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error.Message });
        }

        return Ok(new { Message = "Token revoked successfully" });
    }
 

  
    [HttpPost("Logout")]
   
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var expClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;

            if (string.IsNullOrEmpty(jti) || !long.TryParse(expClaim, out var expTimestamp))
            {
                return BadRequest(new { Error = "Invalid token claims" });
            }
            var expDate = DateTimeOffset.FromUnixTimeSeconds(expTimestamp);
            await _tokenService.BlacklistTokenAsync(jti, expDate);
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });

            return Ok(new
            {
                Success = true,
                Message = "Successfully logged out",
                LogoutTimestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Error processing logout" });
        }
    }

    [HttpGet("current-user")]
    public async Task<ActionResult<AuthResult>> GetCurrentUser()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(userEmail))
        {
            return Unauthorized(new AuthResponse
            {
                Success = false,
                ErrorMessage = "User not authenticated." 
            });
        }

        var result = await _authService.GetCurrentUserAsync(userEmail);

        if (!result.success)
        {
            return NotFound(new AuthResult
            {
                success = false
            });
        }

        return Ok(result);
    }



}


