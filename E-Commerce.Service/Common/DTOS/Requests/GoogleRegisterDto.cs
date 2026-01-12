using System.ComponentModel.DataAnnotations;
namespace E_Commerce.Application.Common.DTOS.Requests;
public class GoogleRegisterDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;
}

