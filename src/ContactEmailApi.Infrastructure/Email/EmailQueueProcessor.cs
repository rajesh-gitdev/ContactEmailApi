using ContactEmailApi.Application.Abstractions.Email;
using ContactEmailApi.Application.Models.Email;
using ContactEmailApi.Domain.Entities;
using ContactEmailApi.Domain.Enums;
using ContactEmailApi.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ContactEmailApi.Infrastructure.Email;

/// <summary>
/// Background service that drains the email queue and delivers each message via
/// <see cref="IEmailService"/>, with a small bounded retry and an audit record written
/// to <see cref="EmailLog"/>. A DI scope is created per message so scoped services
/// (the DbContext) are resolved correctly.
/// </summary>
public sealed class EmailQueueProcessor : BackgroundService
{
    private const int MaxAttempts = 3;

    private readonly IEmailQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailQueueProcessor> _logger;

    public EmailQueueProcessor(
        IEmailQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<EmailQueueProcessor> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email queue processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            EmailMessage message;
            try
            {
                message = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            await ProcessAsync(message, stoppingToken);
        }

        _logger.LogInformation("Email queue processor stopping.");
    }

    private async Task ProcessAsync(EmailMessage message, CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var log = new EmailLog
        {
            ToAddress = message.To.Address,
            Subject = message.Subject,
            TemplateType = message.TemplateType,
            DeliveryStatus = EmailDeliveryStatus.Pending
        };

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            log.AttemptCount = attempt;
            try
            {
                await emailService.SendAsync(message, stoppingToken);
                log.DeliveryStatus = EmailDeliveryStatus.Sent;
                log.SentAtUtc = DateTimeOffset.UtcNow;
                log.ErrorMessage = null;
                break;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                log.DeliveryStatus = EmailDeliveryStatus.Failed;
                log.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to send email to {Recipient} (attempt {Attempt}/{Max}).",
                    message.To.Address, attempt, MaxAttempts);

                if (attempt < MaxAttempts)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // 2s, 4s
                    await Task.Delay(delay, stoppingToken);
                }
            }
        }

        try
        {
            db.Set<EmailLog>().Add(log);
            await db.SaveChangesAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            // Never let audit-log persistence failures crash the processor loop.
            _logger.LogError(ex, "Failed to persist email log for {Recipient}.", message.To.Address);
        }
    }
}
