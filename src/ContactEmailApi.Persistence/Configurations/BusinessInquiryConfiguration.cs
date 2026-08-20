using ContactEmailApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContactEmailApi.Persistence.Configurations;

public sealed class BusinessInquiryConfiguration : IEntityTypeConfiguration<BusinessInquiry>
{
    public void Configure(EntityTypeBuilder<BusinessInquiry> builder)
    {
        builder.ToTable("BusinessInquiries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReferenceCode).HasMaxLength(32).IsRequired();
        builder.HasIndex(x => x.ReferenceCode).IsUnique();

        builder.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ContactName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(40);
        builder.Property(x => x.InquiryType).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Message).HasMaxLength(5000).IsRequired();
        builder.Property(x => x.EstimatedBudget).HasMaxLength(100);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
