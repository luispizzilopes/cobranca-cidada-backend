using System.Text.Json.Serialization;

namespace Cobranca.Cidada.Application.Modules.Auth.Responses;

public record TokenJwtInformationResponse
{
    public string Token { get; init; } = string.Empty;
    public DateTime DateExpiration { get; init; }
}
