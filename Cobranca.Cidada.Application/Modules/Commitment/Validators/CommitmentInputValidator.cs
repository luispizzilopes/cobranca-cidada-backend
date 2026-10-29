using Cobranca.Cidada.Application.Modules.Commitment.Requests;
using Cobranca.Cidada.Domain.Modules.Commitment.Constants;
using Cobranca.Cidada.Domain.Modules.Commitment.Rules;
using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Commitment.Validators;

/// <summary>Regras de entrada de compromisso: fonte única para o seed e para o painel de curadoria.</summary>
public class CommitmentInputValidator : AbstractValidator<CommitmentInput>
{
    private static readonly string[] SourceKinds = ["primary", "press"];

    public CommitmentInputValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("O título é obrigatório.").MaximumLength(300).WithMessage("O título deve ter no máximo 300 caracteres.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("A descrição é obrigatória.");
        RuleFor(x => x.AreaCode).NotEmpty().WithMessage("O código da área é obrigatório.");
        RuleFor(x => x.AreaName).NotEmpty().WithMessage("O nome da área é obrigatório.");
        RuleFor(x => x.OriginName).NotEmpty().WithMessage("A origem é obrigatória.");

        RuleFor(x => x.ExternalId)
            .Must(id => id is null || !string.IsNullOrWhiteSpace(id))
            .WithMessage("O ID externo, quando informado, não pode ser vazio.");

        RuleFor(x => x.DeadlineText)
            .Must(text => text is null || !string.IsNullOrWhiteSpace(text))
            .WithMessage("O texto do prazo deve ser nulo quando não há prazo (não use texto vazio).");

        RuleFor(x => x.Sources).NotEmpty().WithMessage("Todo compromisso precisa de ao menos uma fonte.");
        RuleForEach(x => x.Sources).ChildRules(source =>
        {
            source.RuleFor(s => s.Name).NotEmpty().WithMessage("O nome da fonte é obrigatório.");
            source.RuleFor(s => s.Url).Must(StatusRules.IsValidHttpUrl).WithMessage("URL de fonte inválida.");
            source.RuleFor(s => s.Kind).Must(kind => SourceKinds.Contains(kind)).WithMessage("O tipo da fonte deve ser 'primary' ou 'press'.");
        });

        When(x => x.InitialStatus is not null, () =>
        {
            RuleFor(x => x.InitialStatus!.Status)
                .Must(label => CommitmentStatusLabels.TryParse(label, out _))
                .WithMessage(x => $"Situação inválida: '{x.InitialStatus!.Status}'. Valores permitidos: {string.Join("; ", CommitmentStatusLabels.AllLabels)}.");

            RuleFor(x => x).Custom((input, context) =>
            {
                var status = input.InitialStatus!;
                if (!CommitmentStatusLabels.TryParse(status.Status, out var parsed))
                    return;

                var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
                var violation = StatusRules.Check(parsed, status.ReferenceDate, status.SourceUrl, input.DeadlineDate, status.IsFormalAct, today);
                if (violation is not null)
                    context.AddFailure("InitialStatus", violation);
            });
        });
    }
}
