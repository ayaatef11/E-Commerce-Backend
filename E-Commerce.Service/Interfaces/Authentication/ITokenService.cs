using E_Commerce.Application.Common.DTOS.Responses;
using E_Commerce.Core.Shared.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace E_Commerce.Application.Interfaces.Authentication;
public interface ITokenService
{
    Task<AuthResult> GenerateJwtToken(AppUser user);
    Task<RefreshToken> GenerateRefreshTokenAsync(AppUser user, string jwtId);
    Task<AuthResult> RefreshTokenAsync(string token, string refreshToken);
    Task<Result> RevokeRefreshTokenAsync(string refreshToken);
    Task<bool> IsTokenBlacklistedAsync(string jti);


    public Task BlacklistTokenAsync(string jti, DateTimeOffset expiry);
}
