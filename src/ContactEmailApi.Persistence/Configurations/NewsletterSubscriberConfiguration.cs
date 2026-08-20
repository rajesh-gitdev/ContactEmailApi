using ContactEmailApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContactEmailApi.Persistence.Configurations;

public sealed class NewsletterSubscriberConfiguration : IEntityTypeConfiguration<NewsletterSubscriber>
{
    public void Configure(EntityTypeBuilder<NewsletterSubscriber> builder)
    {
        builder.ToTable("NewsletterSubscribers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        // One subscription per email address.
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.Name).HasMaxLength(150);

        builder.Ignore(x => x.IsActive);
    }
}
