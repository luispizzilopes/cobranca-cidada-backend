using Cobranca.Cidada.Domain.Interfaces.Repositories.Voting;
using Cobranca.Cidada.Domain.Modules.Voting.Enums;
using Cobranca.Cidada.Application.Modules.Voting.Policies;
using Microsoft.Extensions.Configuration;

namespace Cobranca.Cidada.Tests.Application.Commitment;

public class VotePolicyTests
{
    private static VotePolicy Policy(string? min = null)
        => new(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Voting:MinimumPublicVotes"] = min }).Build());

    [Fact]
    public void DefaultMinimumIsTwenty() => Assert.Equal(20, Policy().MinimumPublicVotes);

    [Fact]
    public void CountsAreAlwaysShown()
    {
        var summary = Policy().Summarize(new VoteTally(Guid.NewGuid(), Charges: 3, Good: 2, Bad: 1));

        Assert.Equal(3, summary.Charges);
        Assert.Equal(2, summary.Good);
        Assert.Equal(1, summary.Bad);
        Assert.False(summary.ChargesHidden);
        Assert.False(summary.RatingsHidden);
    }

    [Fact]
    public void NoVotes_ShowZero()
    {
        var summary = Policy().Summarize(null);
        Assert.Equal(0, summary.Charges);
        Assert.Equal(0, summary.Good);
        Assert.Equal(0, summary.Bad);
        Assert.False(summary.ChargesHidden);
    }

    [Theory]
    [InlineData("boa", VoteRating.Good)]
    [InlineData("MA", VoteRating.Bad)]
    [InlineData("má", VoteRating.Bad)]
    public void ParsesRatings(string text, VoteRating expected)
    {
        Assert.True(VotePolicy.TryParseRating(text, out var rating));
        Assert.Equal(expected, rating);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void EmptyRating_IsAllowed(string? text)
    {
        Assert.True(VotePolicy.TryParseRating(text, out var rating));
        Assert.Null(rating);
    }

    [Fact]
    public void UnknownRating_IsRejected() => Assert.False(VotePolicy.TryParseRating("otimo", out _));
}
