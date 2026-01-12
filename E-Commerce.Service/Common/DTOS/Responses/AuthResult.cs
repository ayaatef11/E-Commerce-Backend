namespace E_Commerce.Application.Common.DTOS.Responses;

public record AuthResult
{
    public bool success { get; init; }
    public string Token { get; init; }=string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Error { get; init; } = string.Empty;
    public IList<string> Errors { get; set; }
    public string RefreshToken { get; internal set; }  = string.Empty;
    public bool IsAuthSuccessful { get; internal set; }
    public List<string> Roles { get; internal set; }

    public static AuthResult Success(string token, string userId, string email, string username)
        => new() { success = true, Token = token, UserId = userId, Email = email, Username = username };

    public static AuthResult Failure(string error)
        => new() { success = false, Error = error };
}