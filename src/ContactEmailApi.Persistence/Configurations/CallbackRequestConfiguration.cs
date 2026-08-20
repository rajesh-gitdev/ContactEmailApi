using ContactEmailApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContactEmailApi.Persistence.Configurations;

public sealed class CallbackRequestConfiguration : IEntityTypeConfiguration<CallbackRequest>
{
    public void Configure(EntityTypeBuilder<CallbackRequest> builder)
    {
        builder.ToTable("CallbackRequests");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(40).IsRequired();
        builder.Property(x => x.PreferredTime).HasMaxLength(100);
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
