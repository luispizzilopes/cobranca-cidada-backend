namespace Cobranca.Cidada.Application.Modules.Auth.Requests;

public record ConfirmEmailChangeRequest(string NewEmail, string Code);
