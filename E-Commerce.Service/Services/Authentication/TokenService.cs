using E_Commerce.Core.Data;
using E_Commerce.Application.Common.DTOS.Responses;
using E_Commerce.Application.Interfaces.Authentication;
using E_Commerce.Core.Shared.Results;
using System.Text;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<AppUser> _userManager;
    private readonly StoreContext _dbContext;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly IMemoryCache _cache;

    public TokenService(
        IConfiguration configuration,
        UserManager<AppUser> userManager,
        StoreContext dbContext,
        TokenValidationParameters tokenValidationParameters,
        IMemoryCache cache)
    {
        _configuration = configuration;
        _userManager = userManager;
        _dbContext = dbContext;
        _tokenValidationParameters = tokenValidationParameters;
        _cache = cache;
    }

    // ========== GENERATE JWT TOKEN ==========
    public async Task<AuthResult> GenerateJwtToken(AppUser user)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["JwtConfig:Secret"]!);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),//
           new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName)
        };
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        var userClaims = await _userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(30),  
            Issuer = _configuration["JwtConfig:Issuer"],
            Audience = _configuration["JwtConfig:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = jwtTokenHandler.WriteToken(token);

        // Generate and store refresh token
        var refreshToken = await GenerateRefreshTokenAsync(user, token.Id);

        return new AuthResult
        {
            Token = jwtToken,
            RefreshToken = refreshToken.Token,
            success = true
        };
    }

    // ========== GENERATE REFRESH TOKEN ==========
    public async Task<RefreshToken> GenerateRefreshTokenAsync(AppUser user, string jwtId)
    {
        var existingTokens = await _dbContext.RefreshTokens
      .Where(rt => rt.AppUserId == user.Id)
      .ToListAsync();

        _dbContext.RefreshTokens.RemoveRange(existingTokens);

        var refreshToken = new RefreshToken
        {
            Token = GenerateSecureToken(),
            AppUserId = user.Id,
            AddedDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7), // 7-day expiry
            IsUsed = false,
            IsRevoked = false
        };

        await _dbContext.RefreshTokens.AddAsync(refreshToken);
        await _dbContext.SaveChangesAsync();

        return refreshToken;
    }

    private string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    // ========== REFRESH TOKEN VALIDATION ==========
    public async Task<AuthResult> RefreshTokenAsync(string token, string refreshToken)
    {
        var validatedToken = GetPrincipalFromToken(token);

        if (validatedToken == null)
            return AuthResult.Failure("Invalid token");

        // Check expiry (even if expired, signature must be valid)
        var expiryDateUnix = long.Parse(validatedToken.Claims
            .First(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

        var expiryDate = DateTimeOffset.FromUnixTimeSeconds(expiryDateUnix).UtcDateTime;

        if (expiryDate > DateTime.UtcNow)
            return AuthResult.Failure("Token hasn't expired yet");

        // Check if refresh token exists and is valid
        var storedRefreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshToken);

        if (storedRefreshToken == null)
            return AuthResult.Failure("Refresh token doesn't exist");

        if (storedRefreshToken.IsUsed || storedRefreshToken.IsRevoked)
            return AuthResult.Failure("Refresh token has been used or revoked");

        if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
            return AuthResult.Failure("Refresh token has expired");

        // Get JTI from old token
        var jti = validatedToken.Claims
            .First(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

        // Mark old refresh token as used
        storedRefreshToken.IsUsed = true;
        _dbContext.RefreshTokens.Update(storedRefreshToken);

        // Generate new tokens
        var user = await _userManager.FindByIdAsync(
            validatedToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

        return await GenerateJwtToken(user);
    }

    // ========== REVOKE REFRESH TOKEN ==========
    public async Task<Result> RevokeRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return Result.Failure(new Error { Message = "Refresh token is required" });

        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (storedToken == null)
            return Result.Failure(new Error { Message = "Refresh token not found" });

        if (storedToken.IsRevoked)
            return Result.Failure(new Error { Message = "Token was already revoked" });

        storedToken.IsRevoked = true;
        storedToken.RevokedDate = DateTime.UtcNow;

        _dbContext.RefreshTokens.Update(storedToken);
        await _dbContext.SaveChangesAsync();

        return Result.Success("Refresh token revoked successfully");
    }

    // ========== HELPER METHODS ==========
    private ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var validationParameters = _tokenValidationParameters.Clone();
            validationParameters.ValidateLifetime = false; // Allow expired tokens for refresh flow

            var principal = tokenHandler.ValidateToken(
                token,
                validationParameters,
                out var validatedToken);

            if (!IsJwtWithValidAlgorithm(validatedToken))
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private bool IsJwtWithValidAlgorithm(SecurityToken validatedToken)
    {
        return validatedToken is JwtSecurityToken jwtSecurityToken &&
               jwtSecurityToken.Header.Alg.Equals(
                   SecurityAlgorithms.HmacSha256,
                   StringComparison.InvariantCultureIgnoreCase);
    }
    public Task<bool> IsTokenBlacklistedAsync(string jti)
    {
        return Task.FromResult(_cache.TryGetValue(jti, out _));
    }


    public Task BlacklistTokenAsync(string jti, DateTimeOffset expiry)
    {
        _cache.Set(jti, true, expiry);
        return Task.CompletedTask;
    }
}