using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOS.Auth.Requests;

// Request DTO
public class SendVerificationCodeRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}