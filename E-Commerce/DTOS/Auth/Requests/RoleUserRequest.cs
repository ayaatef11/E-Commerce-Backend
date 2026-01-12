using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOS.Auth.Requests
{
    public class RoleUserRequest
    {
        public string Id { get; set; } // Empty for new roles
        public required string Name { get; set; }
    }
}
