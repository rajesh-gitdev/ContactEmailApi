using ContactEmailApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContactEmailApi.Persistence.Configurations;

public sealed class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.ToTable("EmailLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ToAddress).HasMaxLength(320).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(300).IsRequired();
        builder.Property(x => x.TemplateType).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.DeliveryStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasIndex(x => x.DeliveryStatus);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
