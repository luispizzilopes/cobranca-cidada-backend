using Cobranca.Cidada.Domain.Exceptions;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;
using Cobranca.Cidada.Domain.Modules.Voting.Aggregates;
using Cobranca.Cidada.Domain.Modules.Voting.Enums;

namespace Cobranca.Cidada.Tests.Domain.Voting;

public class VoteTests
{
    private static readonly string Key = new('a', 64);

    [Fact]
    public void Create_KeepsRatingAndCharge()
    {
        var vote = Vote.Create(Key, CommitmentId.CreateIdentifier(), VoteRating.Good, true);

        Assert.Equal(VoteRating.Good, vote.Rating);
        Assert.True(vote.Charge);
        Assert.False(vote.IsEmpty);
    }

    [Fact]
    public void Set_WithoutRatingAndCharge_BecomesEmpty_SoItCanBeUndone()
    {
        var vote = Vote.Create(Key, CommitmentId.CreateIdentifier(), VoteRating.Bad, true);

        vote.Set(null, false);

        Assert.True(vote.IsEmpty);
    }

    [Fact]
    public void Create_WithoutVoterKey_Throws()
        => Assert.Throws<DomainException>(() => Vote.Create(" ", CommitmentId.CreateIdentifier(), null, true));
}
