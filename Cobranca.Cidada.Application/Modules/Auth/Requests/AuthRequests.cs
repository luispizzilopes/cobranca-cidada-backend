namespace Cobranca.Cidada.Application.Modules.Auth.Requests;

public record RequestRegistrationCodeRequest(string Email);

public record RegisterRequest(string Name, string Email, string Password, string Code, bool AcceptedPrivacyPolicy);

public record LoginRequest(string Email, string Password);

public record RefreshTokenRequest(string? RefreshToken);

/// <summary>
/// <paramref name="Credential"/> é o ID token devolvido pelo botão "Entrar com o Google". Quem ainda não tem conta
/// precisa aceitar a política de privacidade (<paramref name="AcceptedPrivacyPolicy"/>) para que a conta seja criada.
/// </summary>
public record GoogleLoginRequest(string Credential, bool AcceptedPrivacyPolicy);

/// <summary>
/// Confirma a identidade para apagar a conta: a senha ou, para quem entra só com o Google e não tem senha,
/// um ID token do Google recém-emitido.
/// </summary>
public record DeleteAccountRequest(string? Password, string? GoogleCredential = null);
