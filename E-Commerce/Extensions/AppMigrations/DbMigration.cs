namespace E_Commerce.Extensions.AppMigrations;
public static class DbMigration
{ 
    public static async Task AddMigrationServices(this WebApplication app, params DbContext[] dbContexts)
    {
        using var scope = app.Services.CreateScope();
        foreach (var context in dbContexts)
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                await context.Database.MigrateAsync();
            }
        }
    }
}

