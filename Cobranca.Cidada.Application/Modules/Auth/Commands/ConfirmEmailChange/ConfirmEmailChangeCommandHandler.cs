using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.UseCases.ConfirmEmailChange;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.ConfirmEmailChange;

public class ConfirmEmailChangeCommandHandler : IRequestHandler<ConfirmEmailChangeCommand, ErrorOr<Success>>
{
    private readonly ConfirmEmailChangeUseCase _useCase;

    public ConfirmEmailChangeCommandHandler(ConfirmEmailChangeUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<Success>> Handle(ConfirmEmailChangeCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.UserId, command.Request, cancellationToken);
}
