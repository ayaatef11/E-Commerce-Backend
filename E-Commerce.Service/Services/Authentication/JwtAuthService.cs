using E_Commerce.Application.Interfaces.Authentication;
using System.Text;
namespace E_Commerce.Application.Services.Authentication;

public class JwtAuthService : IJwtAuthService
{

    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly UserManager<AppUser> _userManager;
    public JwtAuthService(IConfiguration config, UserManager<AppUser> userManager)
    {
        _configuration = config;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtConfig:Secret"]));
        _userManager = userManager;
    }

    public async Task<string> GenerateJwtToken(AppUser user)
    {
        var claims = new List<Claim>
                {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName),
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
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtConfig:ExpiryMinutes"])),
            Issuer = _configuration["JwtConfig:Issuer"],
            Audience = _configuration["JwtConfig:Audience"],
            SigningCredentials = new SigningCredentials(
                       _signingKey,
                       SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
    {
        var tokenOptions = new JwtSecurityToken(
            issuer: _configuration["validIssuer"],
            audience: _configuration["validAudience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["expiryInMinutes"])),
            signingCredentials: signingCredentials);
        return tokenOptions;
    }
    public ClaimsPrincipal ValidateJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["JwtConfig:Key"]);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration["JwtConfig:Issuer"],
            ValidAudience = _configuration["JwtConfig:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public TokenValidationParameters GetValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ValidateIssuer = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = _configuration["Jwt:Audience"],
            ValidateLifetime = true,//it may be false if we want to  refresh the token before it expires

            ClockSkew = TimeSpan.Zero
        };
    }
 
}
 