using E_Commerce.Infrastructure.Persistence.Seeding.JsonParser;

namespace E_Commerce.Extensions.AppSeeding;
public static class DatabaseSeeder
{
    public static async Task SeedDatabaseAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var context = services.GetRequiredService<StoreContext>();
            await IdentitySeeder.SeedRolesAndAdmin(userManager, roleManager);
            await app.AddMigrationServices(context);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database");
        }
    }
}

