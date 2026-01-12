namespace E_Commerce.Infrastructure.Providers;
public class DigitsEmailProvider<TUser> : IUserTwoFactorTokenProvider<TUser>
where TUser : IdentityUser
{
    private static readonly Random _random = new Random();

    public Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
    {
        var code = _random.Next(0, 1000000).ToString("D6");
        return Task.FromResult(code);
    }

    public Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
    {
        return Task.FromResult(token != null && token.Length == 6 && token.All(char.IsDigit));
    }

    public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
    {
        return Task.FromResult(true);
    }
}


