using Cobranca.Cidada.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Cobranca.Cidada.Infrastructure.Services;

/// <summary>
/// Somente desenvolvimento: sem Brevo configurado, imprime o assunto e o corpo no log. Nunca é registrado em Produção.
/// </summary>
public class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var text = System.Text.RegularExpressions.Regex.Replace(htmlBody, "<[^>]+>", " ");
        _logger.LogWarning("[E-MAIL DE DESENVOLVIMENTO] Assunto: {Subject}. Conteúdo: {Body}", subject, System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim());
        return Task.CompletedTask;
    }
}
