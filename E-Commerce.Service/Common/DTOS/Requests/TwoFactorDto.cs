using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Application.Common.DTOS.Requests;
public class TwoFactorDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }=string.Empty; 

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public string Provider { get; set; } = string.Empty;// e.g., "Email", "Authenticator"
}
