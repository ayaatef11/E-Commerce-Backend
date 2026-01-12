using E_Commerce.Application.Common.DTOS.Responses;
namespace E_Commerce.DTOS.Auth.Responses;
public class AuthResponse
{
    public IEnumerable<string> Errors { get; set; }
    public bool Success { get; set; }
    public string? Token { get; set; } 
    public string? UserId { get; set; }  
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    //public List<string> Roles { get; set; }
    public string ErrorMessage { get; set; }
    public AuthResponse()
    {
        
    }
    public AuthResponse(AuthResult result)
    {
        Success = result.success;
        UserId = result.UserId;
        Email = result.Email;
        Token = result.Token;
        Username = result.Username;
    }
}
