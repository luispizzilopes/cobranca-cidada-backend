namespace Cobranca.Cidada.Application.Modules.Auth.Requests;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
