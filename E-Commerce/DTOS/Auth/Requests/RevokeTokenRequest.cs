using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOS.Auth.Requests;
public class RevokeTokenRequest
{
    [Required]
    public string RefreshToken { get; set; }
}
