namespace Cobranca.Cidada.Application.Modules.Comment.Responses;

/// <summary>Comentário raiz em destaque, com o compromisso ao qual pertence. Nunca expõe a chave do votante.</summary>
public record RecentCommentResponse(Guid Id, string Text, DateTime CreatedAtUtc, string CommitmentId, string CommitmentTitle);
