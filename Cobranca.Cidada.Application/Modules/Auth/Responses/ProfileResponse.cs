using System.Text.Json.Serialization;

namespace Cobranca.Cidada.Application.Modules.Auth.Responses;

public record ProfileResponse(
    string UserId,
    string? Email,
    string Name,
    IReadOnlyList<string> Roles,
    string? ConsentVersion,
    DateTime? ConsentAcceptedAtUtc,
    bool HasPassword = true,
    bool GoogleLinked = false);
