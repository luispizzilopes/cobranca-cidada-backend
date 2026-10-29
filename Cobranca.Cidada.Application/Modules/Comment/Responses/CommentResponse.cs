namespace Cobranca.Cidada.Application.Modules.Comment.Responses;

/// <summary>
/// Nunca expõe a chave do votante: só indica se o comentário é da própria pessoa.
/// <paramref name="Replies"/> só vem preenchido nos comentários raiz (respostas em um único nível).
/// </summary>
public record CommentResponse(Guid Id, string Text, DateTime CreatedAtUtc, bool IsMine, Guid? ParentId, IReadOnlyList<CommentResponse> Replies);
