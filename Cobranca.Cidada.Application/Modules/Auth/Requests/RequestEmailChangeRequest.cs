namespace Cobranca.Cidada.Application.Modules.Auth.Requests;

public record RequestEmailChangeRequest(string NewEmail, string? Password, string? GoogleCredential = null);
