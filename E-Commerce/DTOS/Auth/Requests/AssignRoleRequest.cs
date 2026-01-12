using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOS.Auth.Requests;
public class AssignRoleRequest
{
    [Required]
    public string UserId { get; set; }

    [Required]
    public string RoleName { get; set; }
}