using ContactEmailApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContactEmailApi.Persistence.Configurations;

public sealed class ContactSubmissionConfiguration : IEntityTypeConfiguration<ContactSubmission>
{
    public void Configure(EntityTypeBuilder<ContactSubmission> builder)
    {
        builder.ToTable("ContactSubmissions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReferenceCode).HasMaxLength(32).IsRequired();
        builder.HasIndex(x => x.ReferenceCode).IsUnique();

        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(5000).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(40);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
