namespace E_Commerce.Extensions;
public static class EmailExtensions
{
    public static string GetUsernameFromEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return string.Empty;
        return email.Split('@', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
    }
}
