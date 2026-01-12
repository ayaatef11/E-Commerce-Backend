namespace E_Commerce.Core.Models.AuthModels;
public class AppUser : IdentityUser
{
    public string Full_Name { get; set; } = "Admin";
    public string Address { get; set; } = "Cairo";
    public string Job_Title { get; set; } = "Full Stack Developer";
    public string EmailConfirmationCode { get; set; } = "0";
    public DateTime? EmailConfirmationCodeExpiry { get; set; } = DateTime.UtcNow;
    public string PhotoPath { get; set; } = ".";
    public RefreshToken? RefreshToken { get; set; }
}

