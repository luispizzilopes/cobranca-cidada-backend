using System.Net.Http.Json;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Cobranca.Cidada.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Cobranca.Cidada.Infrastructure.Services;

public class BrevoEmailService : IEmailService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly BrevoSettings _settings;

    public BrevoEmailService(IHttpClientFactory httpClientFactory, IOptions<BrevoSettings> settings)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            sender = new { name = _settings.FromName, email = _settings.FromEmail },
            to = new[] { new { email = to } },
            subject,
            htmlContent = htmlBody,
        };

        var client = _httpClientFactory.CreateClient("Brevo");
        var response = await client.PostAsJsonAsync("smtp/email", payload, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
