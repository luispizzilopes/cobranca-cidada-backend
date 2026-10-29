using System.Text.Json;
using Cobranca.Cidada.Domain.Exceptions;
using Cobranca.Cidada.Domain.Modules.Commitment.Enums;

namespace Cobranca.Cidada.Tests.Domain.Commitment;

public class CommitmentTests
{
    private static readonly DateOnly Today = new(2028, 6, 1);
    private const string Url = "https://www.gov.br/fonte";

    [Fact]
    public void Create_StartsAsNotEvaluatedAndPublished()
    {
        var commitment = TestFactories.Commitment();

        Assert.Equal(CommitmentStatus.NotEvaluated, commitment.CurrentStatus);
        Assert.True(commitment.IsPublished);
        Assert.NotNull(commitment.PublishedAtUtc);
        Assert.Equal(1, commitment.Version);
        Assert.Single(commitment.Sources);
    }

    [Fact]
    public void Create_WithoutPublish_StaysUnpublished()
        => Assert.False(TestFactories.Commitment(publish: false).IsPublished);

    [Fact]
    public void RegisterStatus_UpdatesCurrentStatusAndKeepsHistory()
    {
        var commitment = TestFactories.Commitment();

        commitment.RegisterStatus(CommitmentStatus.InPreparation, Today.AddDays(-10), Url, "Anúncio", false, "curador-1", Today);
        commitment.RegisterStatus(CommitmentStatus.InProgress, Today, Url, null, false, "curador-1", Today);

        Assert.Equal(CommitmentStatus.InProgress, commitment.CurrentStatus);
        Assert.Equal(Today, commitment.CurrentStatusDate);
        Assert.Equal(2, commitment.StatusEntries.Count);
    }

    [Fact]
    public void RegisterStatus_WithoutSource_Throws()
    {
        var commitment = TestFactories.Commitment();

        Assert.Throws<DomainException>(() =>
            commitment.RegisterStatus(CommitmentStatus.InProgress, Today, null, null, false, "curador-1", Today));
        Assert.Empty(commitment.StatusEntries);
        Assert.Equal(CommitmentStatus.NotEvaluated, commitment.CurrentStatus);
    }

    [Fact]
    public void RegisterStatus_UnfulfilledBeforeDeadline_Throws()
    {
        var commitment = TestFactories.Commitment(deadlineDate: new DateOnly(2030, 12, 31));

        Assert.Throws<DomainException>(() =>
            commitment.RegisterStatus(CommitmentStatus.Unfulfilled, Today, Url, null, true, "curador-1", Today));
        Assert.Equal(CommitmentStatus.NotEvaluated, commitment.CurrentStatus);
    }

    [Fact]
    public void RegisterStatus_UnfulfilledAfterDeadline_IsAccepted()
    {
        var commitment = TestFactories.Commitment(deadlineDate: new DateOnly(2027, 12, 31));

        commitment.RegisterStatus(CommitmentStatus.Unfulfilled, Today, Url, null, false, "curador-1", Today);

        Assert.Equal(CommitmentStatus.Unfulfilled, commitment.CurrentStatus);
    }

    [Fact]
    public void Update_StoresPreviousContentAsRevisionAndBumpsVersion()
    {
        var commitment = TestFactories.Commitment();
        var originalTitle = commitment.Title;

        commitment.Update(TestFactories.Content("Novo título"), commitment.AreaId, commitment.OriginId, "curador-1", "Correção de redação");

        Assert.Equal("Novo título", commitment.Title);
        Assert.Equal(2, commitment.Version);
        var revision = Assert.Single(commitment.Revisions);
        Assert.Equal(1, revision.Version);
        Assert.Equal("curador-1", revision.AuthorId);
        Assert.Equal("Correção de redação", revision.Reason);
        using var snapshot = JsonDocument.Parse(revision.SnapshotJson);
        Assert.Equal(originalTitle, snapshot.RootElement.GetProperty("Title").GetString());
    }

    [Fact]
    public void Update_WithoutReason_Throws()
    {
        var commitment = TestFactories.Commitment();

        Assert.Throws<DomainException>(() =>
            commitment.Update(TestFactories.Content("Outro"), commitment.AreaId, commitment.OriginId, "curador-1", " "));
    }

    [Fact]
    public void Unpublish_HidesCommitment()
    {
        var commitment = TestFactories.Commitment();
        commitment.Unpublish();
        Assert.False(commitment.IsPublished);
    }
}
