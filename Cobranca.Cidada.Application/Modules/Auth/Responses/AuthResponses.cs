using System.Text.Json.Serialization;

namespace Cobranca.Cidada.Application.Modules.Auth.Responses;

public record TokenJwtInformationResponse
{
    public string Token { get; init; } = string.Empty;
    public DateTime DateExpiration { get; init; }
}

public record LoginResponse(
    string UserId,
    string? Email,
    string Name,
    IReadOnlyList<string> Roles,
    string Token,
    DateTime DateExpiration)
{
    [JsonIgnore]
    public string RefreshToken { get; init; } = string.Empty;

    [JsonIgnore]
    public DateTime RefreshTokenExpiresAtUtc { get; init; }
}

public record ProfileResponse(
    string UserId,
    string? Email,
    string Name,
    IReadOnlyList<string> Roles,
    string? ConsentVersion,
    DateTime? ConsentAcceptedAtUtc,
    bool HasPassword = true,
    bool GoogleLinked = false);
