using Microsoft.AspNetCore.Identity;

namespace E_Commerce.Application.Interfaces.Authentication
{
    public interface IRoleService
    {
        Task<bool> AssignRoleToUser(string userEmail, string roleName);
         Task<IdentityResult> CreateRoleAsync(string roleName);

         Task<IdentityResult> DeleteRoleAsync(string roleName);

         Task<IdentityResult> UpdateRoleAsync(string oldName, string newName);
         List<IdentityRole> GetAllRoles();
        }
}
