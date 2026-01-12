namespace E_Commerce.Core.Data;
public class StoreContext : IdentityDbContext<AppUser>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public StoreContext(DbContextOptions<StoreContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    //dbsets
    public DbSet<ChangeLog> ChangeLogs { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderItem> OrderItems { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public DbSet<Permission> Permissions { get; set; }

    public override int SaveChanges()
    { 
        var result = base.SaveChanges(); 
        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) 
    {
      
        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }
 
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RefreshToken>()
   .HasOne(rt => rt.User)
   .WithOne(u => u.RefreshToken)
   .HasForeignKey<RefreshToken>(rt => rt.AppUserId)
   .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
 
}

