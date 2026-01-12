using System.ComponentModel.DataAnnotations;
namespace E_Commerce.Application.Common.DTOS.Requests;
public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }=string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }=string.Empty;
}