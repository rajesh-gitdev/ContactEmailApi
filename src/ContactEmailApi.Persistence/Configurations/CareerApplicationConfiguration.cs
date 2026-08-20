using ContactEmailApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContactEmailApi.Persistence.Configurations;

public sealed class CareerApplicationConfiguration : IEntityTypeConfiguration<CareerApplication>
{
    public void Configure(EntityTypeBuilder<CareerApplication> builder)
    {
        builder.ToTable("CareerApplications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReferenceCode).HasMaxLength(32).IsRequired();
        builder.HasIndex(x => x.ReferenceCode).IsUnique();

        builder.Property(x => x.FullName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Position).HasMaxLength(150).IsRequired();
        builder.Property(x => x.CoverLetter).HasMaxLength(8000);
        builder.Property(x => x.ResumeUrl).HasMaxLength(500);
        builder.Property(x => x.LinkedInUrl).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.Position);
    }
}
