using ErrorOr;
using Cobranca.Cidada.Application.Modules.Comment.UseCases.DeleteComment;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Comment.Commands.DeleteComment;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, ErrorOr<Success>>
{
    private readonly DeleteCommentUseCase _useCase;

    public DeleteCommentCommandHandler(DeleteCommentUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<Success>> Handle(DeleteCommentCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.CommentId, command.ActorId, cancellationToken);
}
