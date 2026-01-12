namespace E_Commerce.Core.Data.Configurations;
    public class OrderItemConfiguration  : IEntityTypeConfiguration<OrderItem>
    {
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasOne(oi => oi.Product)
    .WithMany()
    .HasForeignKey(oi => oi.ProductId);

        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)");
    }
    }

