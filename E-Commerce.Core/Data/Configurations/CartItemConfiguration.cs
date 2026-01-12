namespace E_Commerce.Core.Data.Configurations;
    public class CartItemConfiguration:IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.Property(p => p.Price)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)"); 

            builder.Property(p => p.Quantity)
                   .IsRequired();
        }
    }
