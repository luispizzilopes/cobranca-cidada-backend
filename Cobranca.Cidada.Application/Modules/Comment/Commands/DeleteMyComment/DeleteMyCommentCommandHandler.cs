using ErrorOr;
using Cobranca.Cidada.Application.Modules.Comment.UseCases.DeleteMyComment;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Comment.Commands.DeleteMyComment;

public class DeleteMyCommentCommandHandler : IRequestHandler<DeleteMyCommentCommand, ErrorOr<Success>>
{
    private readonly DeleteMyCommentUseCase _useCase;

    public DeleteMyCommentCommandHandler(DeleteMyCommentUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<Success>> Handle(DeleteMyCommentCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.VoterKey, command.CommentId, cancellationToken);
}
