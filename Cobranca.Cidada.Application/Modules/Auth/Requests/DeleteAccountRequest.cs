namespace Cobranca.Cidada.Application.Modules.Auth.Requests;

/// <summary>
/// Confirma a identidade para apagar a conta: a senha ou, para quem entra só com o Google e não tem senha,
/// um ID token do Google recém-emitido.
/// </summary>
public record DeleteAccountRequest(string? Password, string? GoogleCredential = null);
