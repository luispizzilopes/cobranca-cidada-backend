namespace Cobranca.Cidada.Application.Common.Emails;

public static class EmailTemplates
{
    public static class Subjects
    {
        public const string RegistrationCode = "Seu código de cadastro — Cobrança Cidadã";
        public const string AccountAlreadyExists = "Você já tem conta — Cobrança Cidadã";
        public const string VoterCode = "Seu código para votar — Cobrança Cidadã";
        public const string PasswordResetCode = "Recuperação de senha — Cobrança Cidadã";
        public const string PasswordChanged = "Sua senha foi alterada — Cobrança Cidadã";
        public const string EmailChangeCode = "Confirme seu novo e-mail — Cobrança Cidadã";
        public const string EmailChangeUnavailable = "Não foi possível usar este e-mail — Cobrança Cidadã";
        public const string EmailChanged = "O e-mail da sua conta foi alterado — Cobrança Cidadã";
    }

    public static string RegistrationCode(string code) => Wrap(
        "Confirme seu cadastro",
        $"""
        <p style="margin:0 0 16px;font-size:16px;line-height:1.5;">Use o código abaixo para concluir seu cadastro.</p>
        {CodeBlock(code)}
        <p style="margin:16px 0 0;color:#555;font-size:14px;">O código vale por <strong>15 minutos</strong>. Se não foi você, ignore este e-mail.</p>
        """);

    public static string AccountAlreadyExists() => Wrap(
        "Você já tem uma conta",
        """
        <p style="margin:0;font-size:16px;line-height:1.5;">Alguém tentou criar um cadastro com este e-mail, mas ele já tem conta. Se foi você, basta entrar com seu e-mail e senha. Se não foi, ignore esta mensagem.</p>
        """);

    public static string VoterCode(string code) => Wrap(
        "Confirme seu e-mail para votar",
        $"""
        <p style="margin:0 0 16px;font-size:16px;line-height:1.5;">Use o código abaixo para confirmar que este e-mail é seu e registrar sua opinião. Não é preciso criar conta.</p>
        {CodeBlock(code)}
        <p style="margin:16px 0 0;color:#555;font-size:14px;">O código vale por <strong>15 minutos</strong>. Seu e-mail não aparece junto com seus votos. Se não foi você, ignore este e-mail.</p>
        """);

    public static string PasswordResetCode(string code) => Wrap(
        "Recuperação de senha",
        $"""
        <p style="margin:0 0 16px;font-size:16px;line-height:1.5;">Recebemos um pedido para redefinir a senha da sua conta. Use o código abaixo para continuar.</p>
        {CodeBlock(code)}
        <p style="margin:16px 0 0;color:#555;font-size:14px;">O código vale por <strong>15 minutos</strong>. Se não foi você, ignore este e-mail: sua senha continua a mesma.</p>
        """);

    public static string PasswordChanged() => Wrap(
        "Sua senha foi alterada",
        """
        <p style="margin:0;font-size:16px;line-height:1.5;">A senha da sua conta acabou de ser alterada e todas as sessões abertas foram encerradas. Se não foi você, use a recuperação de senha agora mesmo.</p>
        """);

    public static string EmailChangeCode(string code) => Wrap(
        "Confirme seu novo e-mail",
        $"""
        <p style="margin:0 0 16px;font-size:16px;line-height:1.5;">Use o código abaixo para confirmar que este será o novo e-mail da sua conta.</p>
        {CodeBlock(code)}
        <p style="margin:16px 0 0;color:#555;font-size:14px;">O código vale por <strong>15 minutos</strong>. Se não foi você, ignore este e-mail.</p>
        """);

    public static string EmailChangeUnavailable() => Wrap(
        "Não foi possível usar este e-mail",
        """
        <p style="margin:0;font-size:16px;line-height:1.5;">Alguém pediu para usar este endereço em outra conta, mas ele já pertence a um cadastro. Se foi você, entre na conta que já usa este e-mail. Se não foi, ignore esta mensagem.</p>
        """);

    public static string EmailChanged(string newEmail) => Wrap(
        "O e-mail da sua conta foi alterado",
        $"""
        <p style="margin:0;font-size:16px;line-height:1.5;">O e-mail da sua conta passou a ser <strong>{System.Net.WebUtility.HtmlEncode(newEmail)}</strong> e as sessões abertas foram encerradas. Se não foi você, entre em contato com a equipe imediatamente.</p>
        """);

    private static string CodeBlock(string code) =>
        $"""<div style="font-size:32px;font-weight:700;letter-spacing:8px;color:#1351b4;background:#f8f8f8;border:1px solid #ccc;padding:16px;text-align:center;">{code}</div>""";

    private static string Wrap(string title, string body) => $"""
        <!doctype html>
        <html lang="pt-BR"><body style="margin:0;background:#f8f8f8;font-family:Rawline,Raleway,Arial,sans-serif;color:#333;">
        <div style="max-width:520px;margin:24px auto;background:#fff;border:1px solid #ccc;">
        <div style="background:#1351b4;color:#fff;padding:16px 24px;font-size:18px;font-weight:700;">Cobrança Cidadã</div>
        <div style="padding:24px;"><h1 style="margin:0 0 16px;font-size:22px;">{title}</h1>{body}</div>
        <div style="padding:16px 24px;border-top:1px solid #ccc;color:#555;font-size:12px;">Site de cidadãos, sem vínculo com o governo.</div>
        </div></body></html>
        """;
}
