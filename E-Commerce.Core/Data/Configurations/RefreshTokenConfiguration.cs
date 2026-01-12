namespace E_Commerce.Core.Data.Configurations;


public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(256); 

        builder.Property(rt => rt.IsUsed)
            .IsRequired();

        builder.Property(rt => rt.AddedDate)
            .IsRequired();

        builder.Property(rt => rt.ExpiryDate)
            .IsRequired();

        builder.HasIndex(rt => rt.Token)
            .IsUnique();

        builder.HasOne(rt => rt.User)
            .WithOne(u => u.RefreshToken)
            .OnDelete(DeleteBehavior.Cascade);
    }

}