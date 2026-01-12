using Causmatic_backEnd.Core.Data;
using Causmatic_backEnd.Core.Models.AuthModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Causmatic_backEnd.IntegrationTesting.Databases;

public class DatabaseFixture : IAsyncLifetime
{
    public StoreContext Context { get; }
    string connectionString = $"Server=.;Database=Cosmatic_backEnd;TrustServerCertificate = True;Encrypt= false;Integrated Security=SSPI";


    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<StoreContext>()
            .UseSqlServer(connectionString)
            .Options;
        HttpContextAccessor httpContextAccessor = new HttpContextAccessor();
        Context = new StoreContext(options, httpContextAccessor);
        Context.Database.EnsureCreated();
    }

    public void Dispose() => Context.Database.EnsureDeleted();

    public void SeedTestData(StoreContext context)
    {
        context.Users.Add(new AppUser {  Full_Name = "Test User" });
        context.SaveChanges();
    }

    public async Task DisposeAsync()
    {
        await Context.Database.EnsureDeletedAsync();
        await Context.DisposeAsync();
    }


    public async Task InitializeAsync()
    {
        
        await Context.Database.MigrateAsync();
    }
}

