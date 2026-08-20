using ContactEmailApi.Domain.Common;
using ContactEmailApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContactEmailApi.Persistence;

/// <summary>
/// EF Core context for the Contact &amp; Email API. Exposes the submission, subscriber,
/// OTP, and email-audit entity sets; entity configurations are applied from this assembly.
/// </summary>
public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ContactSubmission> ContactSubmissions => Set<ContactSubmission>();
    public DbSet<BusinessInquiry> BusinessInquiries => Set<BusinessInquiry>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<CareerApplication> CareerApplications => Set<CareerApplication>();
    public DbSet<NewsletterSubscriber> NewsletterSubscribers => Set<NewsletterSubscriber>();
    public DbSet<FeedbackEntry> FeedbackEntries => Set<FeedbackEntry>();
    public DbSet<CallbackRequest> CallbackRequests => Set<CallbackRequest>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    /// <summary>Stamps the audit timestamp on modified entities before saving.</summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
