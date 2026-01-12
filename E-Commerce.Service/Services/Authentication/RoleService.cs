using E_Commerce.Application.Interfaces.Authentication;

namespace E_Commerce.Application.Services.Authentication;
    public class RoleService(UserManager<AppUser> _userManager, RoleManager<IdentityRole> _roleManager) : IRoleService
    {
        public async Task<bool> AssignRoleToUser(string userEmail, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
                return false;

            var roleExist = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
                await CreateRoleAsync(roleName);
            if (await _userManager.IsInRoleAsync(user, roleName))
                return true;

            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (!result.Succeeded)
                return false;

            return true;
        }

        public async Task<IdentityResult> CreateRoleAsync(string roleName)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
                throw new Exception("Role already exists");

            return await _roleManager.CreateAsync(new IdentityRole(roleName));
        }

        public async Task<IdentityResult> DeleteRoleAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
                throw new Exception("Role not found");

            return await _roleManager.DeleteAsync(role);
        }

        public async Task<IdentityResult> UpdateRoleAsync(string oldName, string newName)
        {
            var role = await _roleManager.FindByNameAsync(oldName);

            if (role == null)
                throw new Exception("Role not found");

            role.Name = newName;
            role.NormalizedName = newName.ToUpper();

            return await _roleManager.UpdateAsync(role);
        }
        public List<IdentityRole> GetAllRoles()
        {
            return _roleManager.Roles.ToList();
        }
    }
