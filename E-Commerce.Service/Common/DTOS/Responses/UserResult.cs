namespace E_Commerce.Application.Common.DTOS.Responses;
public class UserResult
{
    public bool success { get; set; } = true;
    public string UserId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public IEnumerable<string>? Roles { get; set; }
    public string Error { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string Full_Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Job_Title { get; set; } = null!;
    public string? PhotoPath { get; set; }
    public string? PhoneNumber { get; set; } = null!;

    public static UserResult Failure(string error)
        => new UserResult
        {
            success = false,
            Error = error
        };
}

