namespace Cobranca.Cidada.Application.Modules.Auth.Requests;

public record RequestPasswordResetRequest(string Email);

public record ConfirmPasswordResetRequest(string Email, string Code, string NewPassword);

public record RequestEmailChangeRequest(string NewEmail, string? Password, string? GoogleCredential = null);

public record ConfirmEmailChangeRequest(string NewEmail, string Code);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
