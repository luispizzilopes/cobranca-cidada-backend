using Cobranca.Cidada.BuildingBlocks.Common.Class.AggregateRoot;
using Cobranca.Cidada.Domain.Exceptions;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;
using Cobranca.Cidada.Domain.Modules.Voting.Enums;
using Cobranca.Cidada.Domain.Modules.Voting.ValueObjects;

namespace Cobranca.Cidada.Domain.Modules.Voting.Aggregates;

/// <summary>
/// Opinião de uma pessoa sobre um compromisso. Uma linha por (VoterKey, compromisso).
/// A identidade é o HMAC do e-mail verificado: o voto não guarda e-mail nem usuário.
/// </summary>
public class Vote : AggregateRoot<VoteId>
{
    public string VoterKey { get; private set; } = null!;
    public CommitmentId CommitmentId { get; private set; } = null!;
    public VoteRating? Rating { get; private set; }
    public bool Charge { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>Verdadeiro quando o voto não expressa nada e deve ser removido (desfazer).</summary>
    public bool IsEmpty => Rating is null && !Charge;

    private Vote() { }

    private Vote(string voterKey, CommitmentId commitmentId, VoteRating? rating, bool charge)
        : base(VoteId.CreateIdentifier())
    {
        VoterKey = voterKey;
        CommitmentId = commitmentId;
        Rating = rating;
        Charge = charge;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static Vote Create(string voterKey, CommitmentId commitmentId, VoteRating? rating, bool charge)
    {
        if (string.IsNullOrWhiteSpace(voterKey)) throw new DomainException("A identidade do votante é obrigatória.");
        if (rating is not null && !Enum.IsDefined(rating.Value)) throw new DomainException("Avaliação inválida.");
        return new Vote(voterKey, commitmentId, rating, charge);
    }

    /// <summary>Passa o voto para outra identidade (troca de e-mail da conta).</summary>
    public void Rekey(string newVoterKey)
    {
        if (string.IsNullOrWhiteSpace(newVoterKey)) throw new DomainException("A identidade do votante é obrigatória.");
        VoterKey = newVoterKey;
    }

    public void Set(VoteRating? rating, bool charge)
    {
        if (rating is not null && !Enum.IsDefined(rating.Value)) throw new DomainException("Avaliação inválida.");
        Rating = rating;
        Charge = charge;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
