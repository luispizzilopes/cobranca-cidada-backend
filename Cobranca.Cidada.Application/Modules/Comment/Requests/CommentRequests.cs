namespace Cobranca.Cidada.Application.Modules.Comment.Requests;

/// <param name="ParentId">Comentário que está sendo respondido. Nulo cria um comentário raiz (sujeito ao limite por pessoa).</param>
public record AddCommentRequest(string Text, Guid? ParentId = null);
