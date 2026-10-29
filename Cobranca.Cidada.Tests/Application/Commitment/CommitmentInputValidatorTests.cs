using Cobranca.Cidada.Application.Modules.Commitment.Requests;
using Cobranca.Cidada.Application.Modules.Commitment.Validators;

namespace Cobranca.Cidada.Tests.Application.Commitment;

public class CommitmentInputValidatorTests
{
    private static readonly FakeTimeProvider Clock = new(new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.Zero));
    private readonly CommitmentInputValidator _validator = new(Clock);

    private static CommitmentInput Valid(
        string? deadlineText = null,
        InitialStatusInput? status = null,
        IReadOnlyList<SourceInput>? sources = null) => new(
        "LULA26-ECON-001", "ECON-001", "ECON", "Economia", "Plano de governo",
        "Título", "Descrição", null, null, false, null, null, deadlineText, null, null, [], [], null,
        sources ?? [new SourceInput("Plano", "https://www.tse.jus.br/plano", "49", "primary")],
        status ?? new InitialStatusInput("Não avaliado", new DateOnly(2026, 10, 26), null, null, false));

    [Fact]
    public void ValidInput_Passes() => Assert.True(_validator.Validate(Valid()).IsValid);

    [Fact]
    public void EmptyDeadlineText_IsRejected_BecauseNoDeadlineMustBeNull()
        => Assert.False(_validator.Validate(Valid(deadlineText: "  ")).IsValid);

    [Fact]
    public void InvalidSourceUrl_IsRejected()
        => Assert.False(_validator.Validate(Valid(sources: [new SourceInput("X", "isto-nao-e-url", null, "press")])).IsValid);

    [Fact]
    public void MissingSources_IsRejected()
        => Assert.False(_validator.Validate(Valid(sources: [])).IsValid);

    [Fact]
    public void UnknownStatus_IsRejected()
        => Assert.False(_validator.Validate(Valid(status: new InitialStatusInput("Quase cumprido", new DateOnly(2026, 10, 26), null, null, false))).IsValid);

    [Fact]
    public void ProgressStatusWithoutSourceUrl_IsRejected()
        => Assert.False(_validator.Validate(Valid(status: new InitialStatusInput("Em andamento", new DateOnly(2026, 10, 26), null, null, false))).IsValid);

    [Fact]
    public void UnfulfilledWithoutDeadlineOrFormalAct_IsRejected()
        => Assert.False(_validator.Validate(Valid(status: new InitialStatusInput("Descumprido", new DateOnly(2026, 10, 26), "https://www.gov.br/x", null, false))).IsValid);

    [Fact]
    public void FutureReferenceDate_IsRejected()
        => Assert.False(_validator.Validate(Valid(status: new InitialStatusInput("Não avaliado", new DateOnly(2026, 10, 27), null, null, false))).IsValid);
}

internal sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
