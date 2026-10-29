using Cobranca.Cidada.BuildingBlocks.Pagination;
using CommentAggregate = Cobranca.Cidada.Domain.Modules.Comment.Aggregates.Comment;

namespace Cobranca.Cidada.Domain.Interfaces.Repositories.Comment;

/// <summary>Comentário raiz de um compromisso publicado, com o identificador público e o título dele.</summary>
public record RecentCommentRef(CommentAggregate Comment, string CommitmentSlug, string CommitmentTitle);

public interface ICommentRepository
{
    /// <summary>Comentários raiz do compromisso, do mais recente ao mais antigo.</summary>
    Task<PaginedResult<CommentAggregate>> GetByCommitmentAsync(Guid commitmentId, PaginationParams pagination, CancellationToken cancellationToken);
    /// <summary>Respostas dos comentários raiz informados, da mais antiga à mais recente.</summary>
    Task<IReadOnlyList<CommentAggregate>> GetRepliesAsync(IEnumerable<Guid> rootIds, CancellationToken cancellationToken);
    /// <summary>Último comentário raiz de cada compromisso publicado (um por compromisso), do mais recente ao mais antigo. Respostas ficam de fora.</summary>
    Task<IReadOnlyList<RecentCommentRef>> GetRecentAsync(int limit, CancellationToken cancellationToken);
    Task<CommentAggregate?> GetAsync(Guid commentId, CancellationToken cancellationToken);
    /// <summary>Quantos comentários raiz a pessoa já fez no compromisso (limite por pessoa). Respostas não contam.</summary>
    Task<int> CountByVoterAsync(string voterKey, Guid commitmentId, CancellationToken cancellationToken);
    Task CreateAsync(CommentAggregate comment, CancellationToken cancellationToken);
    void Remove(CommentAggregate comment);
    /// <summary>Move os comentários de uma chave para outra (troca de e-mail). Não confirma; quem chama confirma pela unidade de trabalho.</summary>
    Task MoveAsync(string fromVoterKey, string toVoterKey, CancellationToken cancellationToken);
    Task<int> DeleteAllByVoterAsync(string voterKey, CancellationToken cancellationToken);
}
