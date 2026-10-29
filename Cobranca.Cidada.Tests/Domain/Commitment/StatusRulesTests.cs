using Cobranca.Cidada.Domain.Modules.Commitment.Enums;
using Cobranca.Cidada.Domain.Modules.Commitment.Rules;

namespace Cobranca.Cidada.Tests.Domain.Commitment;

public class StatusRulesTests
{
    private static readonly DateOnly Today = new(2028, 6, 1);
    private const string Url = "https://www.gov.br/fonte";

    [Fact]
    public void NotEvaluated_DoesNotRequireSource()
        => Assert.Null(StatusRules.Check(CommitmentStatus.NotEvaluated, Today, null, null, false, Today));

    [Theory]
    [InlineData(CommitmentStatus.InProgress)]
    [InlineData(CommitmentStatus.Fulfilled)]
    [InlineData(CommitmentStatus.InPreparation)]
    public void AnyOtherStatus_RequiresValidSourceUrl(CommitmentStatus status)
    {
        Assert.NotNull(StatusRules.Check(status, Today, null, null, false, Today));
        Assert.NotNull(StatusRules.Check(status, Today, "não é url", null, false, Today));
        Assert.NotNull(StatusRules.Check(status, Today, "ftp://servidor/arquivo", null, false, Today));
        Assert.Null(StatusRules.Check(status, Today, Url, null, false, Today));
    }

    [Fact]
    public void Unfulfilled_IsRejectedBeforeDeadline()
    {
        var deadline = new DateOnly(2028, 12, 31);
        Assert.NotNull(StatusRules.Check(CommitmentStatus.Unfulfilled, Today, Url, deadline, true, Today));
    }

    [Fact]
    public void Unfulfilled_IsRejectedOnTheDeadlineItself()
    {
        var deadline = Today;
        Assert.NotNull(StatusRules.Check(CommitmentStatus.Unfulfilled, Today, Url, deadline, false, Today));
    }

    [Fact]
    public void Unfulfilled_IsAllowedAfterDeadline()
    {
        var deadline = new DateOnly(2028, 5, 31);
        Assert.Null(StatusRules.Check(CommitmentStatus.Unfulfilled, Today, Url, deadline, false, Today));
    }

    [Fact]
    public void Unfulfilled_WithoutDeadline_RequiresFormalAct()
    {
        Assert.NotNull(StatusRules.Check(CommitmentStatus.Unfulfilled, Today, Url, null, false, Today));
        Assert.Null(StatusRules.Check(CommitmentStatus.Unfulfilled, Today, Url, null, true, Today));
    }

    [Fact]
    public void ReferenceDate_CannotBeInTheFuture()
        => Assert.NotNull(StatusRules.Check(CommitmentStatus.NotEvaluated, Today.AddDays(1), null, null, false, Today));
}
