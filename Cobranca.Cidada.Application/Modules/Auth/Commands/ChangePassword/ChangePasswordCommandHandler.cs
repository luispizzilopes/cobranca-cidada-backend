using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.UseCases.ChangePassword;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ErrorOr<Success>>
{
    private readonly ChangePasswordUseCase _useCase;

    public ChangePasswordCommandHandler(ChangePasswordUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<Success>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.UserId, command.Request, cancellationToken);
}
