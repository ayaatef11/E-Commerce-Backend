using E_Commerce.Core.Shared.Utilties.Identity;

namespace E_Commerce.Infrastructure.Persistence.Seeding.JsonParser;
public class IdentitySeeder
{
    private static bool IsAuthenticated(AppUser user)
    {
        if (!user.EmailConfirmed) return false;
        return true;
    }
    public static async Task SeedRolesAndAdmin(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        try
        {
            var existingAdmin = await userManager.FindByEmailAsync("causmaticstore@gmail.com");
            if (existingAdmin != null)
            {
                if (!IsAuthenticated(existingAdmin))
                {
                    existingAdmin.EmailConfirmed = true;
                    existingAdmin.PhoneNumberConfirmed = false;
                    existingAdmin.TwoFactorEnabled = false;
                    await userManager.UpdateAsync(existingAdmin);
                }
                return;
            }
            string[] roles = { Roles.Admin, Roles.User };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var newAdmin = new AppUser
            {
                UserName = "causmaticstore",
                Email = "causmaticstore@gmail.com",
                Full_Name = "System Admin",
                PhoneNumber = "+201066306608",
                Address = "Cairo",
                Job_Title = "Full_Stack",
                EmailConfirmed = true,
                TwoFactorEnabled = false,
                PhoneNumberConfirmed = false,
                            
            };
            var result = await userManager.CreateAsync(newAdmin, "asdfg12345A");

            if (result.Succeeded)
            {
                var result2 = await userManager.AddToRoleAsync(newAdmin, Roles.Admin);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in IdentitySeeder: {ex.Message}");
        }
    }
}
