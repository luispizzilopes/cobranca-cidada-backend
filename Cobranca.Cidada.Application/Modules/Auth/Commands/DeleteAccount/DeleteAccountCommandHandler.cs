using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.UseCases.DeleteAccount;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.DeleteAccount;

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, ErrorOr<Success>>
{
    private readonly DeleteAccountUseCase _useCase;

    public DeleteAccountCommandHandler(DeleteAccountUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<Success>> Handle(DeleteAccountCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.UserId, command.Request, cancellationToken);
}
