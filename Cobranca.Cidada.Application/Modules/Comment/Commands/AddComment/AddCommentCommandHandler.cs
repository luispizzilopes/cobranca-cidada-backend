using ErrorOr;
using Cobranca.Cidada.Application.Modules.Comment.Responses;
using Cobranca.Cidada.Application.Modules.Comment.UseCases.AddComment;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Comment.Commands.AddComment;

public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, ErrorOr<CommentResponse>>
{
    private readonly AddCommentUseCase _useCase;

    public AddCommentCommandHandler(AddCommentUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<CommentResponse>> Handle(AddCommentCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.VoterKey, command.Slug, command.Request, cancellationToken);
}
