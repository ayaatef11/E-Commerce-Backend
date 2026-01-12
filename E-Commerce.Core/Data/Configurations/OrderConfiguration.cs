namespace E_Commerce.Core.Data.Configurations;
    public class OrderConfiguration:IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order>builder)
        {
            builder.Property(o => o.Status)
                .HasConversion(
                OStatus => OStatus.ToString(),
                OStatus => (OrderStatus)Enum.Parse(typeof(OrderStatus), OStatus)
                );

            builder.Property(p => p.Price)
                   .HasColumnType("decimal(18,2)");

        }
    }

