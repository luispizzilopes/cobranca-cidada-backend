using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.UseCases.ConfirmPasswordReset;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.ConfirmPasswordReset;

public class ConfirmPasswordResetCommandHandler : IRequestHandler<ConfirmPasswordResetCommand, ErrorOr<Success>>
{
    private readonly ConfirmPasswordResetUseCase _useCase;

    public ConfirmPasswordResetCommandHandler(ConfirmPasswordResetUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<Success>> Handle(ConfirmPasswordResetCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.Request, cancellationToken);
}
