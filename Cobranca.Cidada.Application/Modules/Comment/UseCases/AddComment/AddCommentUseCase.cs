using ErrorOr;
using Cobranca.Cidada.Application.Common;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Comment.Requests;
using Cobranca.Cidada.Application.Modules.Comment.Responses;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.Comment.ValueObjects;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;
using CommentAggregate = Cobranca.Cidada.Domain.Modules.Comment.Aggregates.Comment;

namespace Cobranca.Cidada.Application.Modules.Comment.UseCases.AddComment;

/// <summary>
/// Registra um comentário de quem tem conta ou e-mail verificado. Nunca mexe na situação de execução.
/// O limite por pessoa vale só para comentários raiz; respostas não têm limite (só o rate limit da rota).
/// </summary>
public class AddCommentUseCase : IUseCase
{
    /// <summary>Teto por pessoa em cada compromisso, para conter repetição em massa.</summary>
    public const int MaxPerPersonPerCommitment = 5;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ProfanityFilter _profanityFilter;

    public AddCommentUseCase(IUnitOfWork unitOfWork, ProfanityFilter profanityFilter)
    {
        _unitOfWork = unitOfWork;
        _profanityFilter = profanityFilter;
    }

    public async Task<ErrorOr<CommentResponse>> ExecuteAsync(string voterKey, string slug, AddCommentRequest request, CancellationToken cancellationToken = default)
    {
        var commitmentId = await _unitOfWork.CommitmentRepository.GetPublishedIdBySlugAsync(slug.Trim(), cancellationToken);
        if (commitmentId is null)
            return Error.NotFound("Commitment.NotFound", "Compromisso não encontrado.");

        CommentId? parentId = null;
        if (request.ParentId is { } requestedParent)
        {
            var parent = await _unitOfWork.CommentRepository.GetAsync(requestedParent, cancellationToken);
            if (parent is null || parent.CommitmentId.Value != commitmentId.Value)
                return Error.NotFound("Comment.ParentNotFound", "Comentário respondido não encontrado.");

            parentId = parent.ParentId ?? parent.Id;
        }
        else if (await _unitOfWork.CommentRepository.CountByVoterAsync(voterKey, commitmentId.Value, cancellationToken) >= MaxPerPersonPerCommitment)
        {
            return Error.Validation("Comment.LimitReached", $"Você já fez {MaxPerPersonPerCommitment} comentários neste compromisso. Apague um para escrever outro. Responder a outros comentários não tem limite.");
        }

        var text = await _profanityFilter.CensorAsync(request.Text, cancellationToken);
        var comment = CommentAggregate.Create(voterKey, CommitmentId.Create(commitmentId.Value), text, parentId);
        await _unitOfWork.CommentRepository.CreateAsync(comment, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new CommentResponse(comment.Id.Value, comment.Text, comment.CreatedAtUtc, true, comment.ParentId?.Value, []);
    }
}
