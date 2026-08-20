using ContactEmailApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContactEmailApi.Persistence.Configurations;

public sealed class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
{
    public void Configure(EntityTypeBuilder<OtpCode> builder)
    {
        builder.ToTable("OtpCodes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.CodeHash).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Purpose).HasConversion<string>().HasMaxLength(24);

        // Lookup latest active code by email + purpose.
        builder.HasIndex(x => new { x.Email, x.Purpose });
        builder.HasIndex(x => x.ExpiresAtUtc);

        builder.Ignore(x => x.IsConsumed);
    }
}
