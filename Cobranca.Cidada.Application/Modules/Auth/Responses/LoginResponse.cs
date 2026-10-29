using System.Text.Json.Serialization;

namespace Cobranca.Cidada.Application.Modules.Auth.Responses;

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
