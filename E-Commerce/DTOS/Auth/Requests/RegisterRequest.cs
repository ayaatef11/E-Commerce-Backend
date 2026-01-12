namespace E_Commerce.DTOS.Auth.Requests;
public class RegisterRequest
{
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
}
