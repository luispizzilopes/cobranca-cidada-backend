using Cobranca.Cidada.BuildingBlocks.Common.Class.AggregateRoot;
using Cobranca.Cidada.Domain.Exceptions;
using Cobranca.Cidada.Domain.Modules.Comment.ValueObjects;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;

namespace Cobranca.Cidada.Domain.Modules.Comment.Aggregates;

/// <summary>
/// Comentário de uma pessoa sobre um compromisso. É opinião: nunca altera a situação de execução.
/// A autoria é só a chave pseudônima do votante (HMAC do e-mail verificado); o comentário não guarda e-mail nem nome.
/// </summary>
public class Comment : AggregateRoot<CommentId>
{
    public const int MinLength = 3;
    public const int MaxLength = 1000;

    public string VoterKey { get; private set; } = null!;
    public CommitmentId CommitmentId { get; private set; } = null!;
    public string Text { get; private set; } = null!;
    /// <summary>Comentário raiz ao qual este responde; nulo nos comentários raiz. As respostas ficam em um só nível.</summary>
    public CommentId? ParentId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Comment() { }

    private Comment(string voterKey, CommitmentId commitmentId, string text, CommentId? parentId)
        : base(CommentId.CreateIdentifier())
    {
        VoterKey = voterKey;
        CommitmentId = commitmentId;
        Text = text;
        ParentId = parentId;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Comment Create(string voterKey, CommitmentId commitmentId, string text, CommentId? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(voterKey)) throw new DomainException("A identidade do votante é obrigatória.");

        var trimmed = text?.Trim() ?? string.Empty;
        if (trimmed.Length < MinLength) throw new DomainException($"O comentário deve ter ao menos {MinLength} caracteres.");
        if (trimmed.Length > MaxLength) throw new DomainException($"O comentário deve ter no máximo {MaxLength} caracteres.");

        return new Comment(voterKey, commitmentId, trimmed, parentId);
    }

    /// <summary>Passa o comentário para outra identidade (troca de e-mail da conta).</summary>
    public void Rekey(string newVoterKey)
    {
        if (string.IsNullOrWhiteSpace(newVoterKey)) throw new DomainException("A identidade do votante é obrigatória.");
        VoterKey = newVoterKey;
    }
}
