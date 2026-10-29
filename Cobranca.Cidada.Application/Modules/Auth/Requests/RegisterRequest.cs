namespace Cobranca.Cidada.Application.Modules.Auth.Requests;

public record RegisterRequest(string Name, string Email, string Password, string Code, bool AcceptedPrivacyPolicy);
