namespace E_Commerce.Application.Common.DTOS.Responses;

public class LoginResultDto
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public bool RequiresRegistration { get; set; }
}