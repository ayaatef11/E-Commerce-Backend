namespace E_Commerce.Core.Data.Configurations;
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Id)
     .UseIdentityColumn();

        builder.Property(p => p.Name)
            .IsRequired();

        builder.Property(p => p.Description)
            .IsRequired();

        builder.Property(p => p.PictureUrl)
            .IsRequired();

        builder.Property(p => p.Cost)
            .HasColumnType("decimal(18,2)");
        builder.Property(p => p.Price)
               .HasColumnType("decimal(18,2)");
        builder.Property(p => p.ListPrice)
               .HasColumnType("decimal(18,2)");
        builder.Property(p => p.Gomla)
               .HasColumnType("decimal(18,2)");
        builder.Property(p => p.Mandop)
               .HasColumnType("decimal(18,2)");
    }
}

