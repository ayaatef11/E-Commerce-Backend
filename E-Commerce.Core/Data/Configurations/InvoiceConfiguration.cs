namespace E_Commerce.Core.Data.Configurations;
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.Property(b => b.TotalAmount).HasColumnType("decimal(18,2)");
        }
    }

