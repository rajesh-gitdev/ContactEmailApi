using ContactEmailApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContactEmailApi.Persistence.Configurations;

public sealed class FeedbackEntryConfiguration : IEntityTypeConfiguration<FeedbackEntry>
{
    public void Configure(EntityTypeBuilder<FeedbackEntry> builder)
    {
        builder.ToTable("FeedbackEntries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(150);
        builder.Property(x => x.Email).HasMaxLength(320);
        builder.Property(x => x.Message).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
