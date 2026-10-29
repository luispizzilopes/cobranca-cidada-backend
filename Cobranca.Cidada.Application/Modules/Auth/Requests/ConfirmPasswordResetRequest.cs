namespace Cobranca.Cidada.Application.Modules.Auth.Requests;

public record ConfirmPasswordResetRequest(string Email, string Code, string NewPassword);
