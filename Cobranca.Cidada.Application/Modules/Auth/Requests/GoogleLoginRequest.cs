namespace Cobranca.Cidada.Application.Modules.Auth.Requests;

/// <summary>
/// <paramref name="Credential"/> é o ID token devolvido pelo botão "Entrar com o Google". Quem ainda não tem conta
/// precisa aceitar a política de privacidade (<paramref name="AcceptedPrivacyPolicy"/>) para que a conta seja criada.
/// </summary>
public record GoogleLoginRequest(string Credential, bool AcceptedPrivacyPolicy);
