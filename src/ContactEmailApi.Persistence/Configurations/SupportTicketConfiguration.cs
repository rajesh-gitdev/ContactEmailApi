using ContactEmailApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContactEmailApi.Persistence.Configurations;

public sealed class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
{
    public void Configure(EntityTypeBuilder<SupportTicket> builder)
    {
        builder.ToTable("SupportTickets");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TicketNumber).HasMaxLength(24).IsRequired();
        builder.HasIndex(x => x.TicketNumber).IsUnique();

        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.Category).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Subject).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
