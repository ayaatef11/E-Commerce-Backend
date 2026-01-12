namespace E_Commerce.DTOS.Auth.Responses;
    public class UserResponse
    {
    public string Full_Name { get; set; } = null!;
    public string? Address { get; set; }
    public string? Job_Title { get; set; } 
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; } 
}

