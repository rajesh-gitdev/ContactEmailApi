using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace ContactEmailApi.IntegrationTests;

public sealed class EndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public EndpointTests(TestWebApplicationFactory factory) => _factory = factory;

    private HttpClient CreateClient(bool authenticated)
    {
        var client = _factory.CreateClient();
        if (authenticated)
        {
            client.DefaultRequestHeaders.Add("X-Api-Key", TestAuth.ApiKey);
        }

        return client;
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await CreateClient(authenticated: false).GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Ping_IsAnonymous()
    {
        var response = await CreateClient(authenticated: false).GetAsync("/api/v1/diagnostics/ping");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ContactSend_WithoutApiKey_IsUnauthorized()
    {
        var response = await CreateClient(authenticated: false).PostAsJsonAsync("/api/v1/contact/send", new
        {
            name = "Ada",
            email = "ada@example.com",
            subject = "Hi",
            message = "This is a sufficiently long message body."
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ContactSend_WithApiKey_IsAccepted()
    {
        var response = await CreateClient(authenticated: true).PostAsJsonAsync("/api/v1/contact/send", new
        {
            name = "Ada Lovelace",
            email = "ada@example.com",
            subject = "Hello",
            message = "I would like to learn more about your offering, thank you."
        });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task ContactSend_WithHoneypot_IsRejected()
    {
        var response = await CreateClient(authenticated: true).PostAsJsonAsync("/api/v1/contact/send", new
        {
            name = "Bot",
            email = "bot@example.com",
            subject = "Spam",
            message = "This message should be rejected by the honeypot rule.",
            honeypot = "i-am-a-bot"
        });

        // Honeypot trips validation (422) via the validator.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Newsletter_WithoutConsent_IsUnprocessable()
    {
        var response = await CreateClient(authenticated: true).PostAsJsonAsync("/api/v1/newsletter/subscribe", new
        {
            email = "reader@example.com",
            consent = false
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
