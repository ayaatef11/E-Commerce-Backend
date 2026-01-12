using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOS.Auth.Requests;
public class RefreshTokenRequest
{
    [Required]
    public string Token { get; set; }

    [Required]
    public string RefreshToken { get; set; }
}