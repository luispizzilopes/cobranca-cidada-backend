using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Application.Modules.Commitment.UseCases.SeedCommitments;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Commitment.Commands.SeedCommitments;

public class SeedCommitmentsCommandHandler : IRequestHandler<SeedCommitmentsCommand, ErrorOr<SeedReportResponse>>
{
    private readonly SeedCommitmentsUseCase _useCase;

    public SeedCommitmentsCommandHandler(SeedCommitmentsUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<SeedReportResponse>> Handle(SeedCommitmentsCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command, cancellationToken);
}
