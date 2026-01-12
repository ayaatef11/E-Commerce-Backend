namespace E_Commerce.Infrastructure.Persistence.Interceptors;
public class AuditContext : DbContext
{
    public DbSet<AuditEntry> AuditEntries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Your_Connection_String");
    }
}
