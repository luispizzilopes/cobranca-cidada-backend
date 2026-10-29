using ErrorOr;
using Cobranca.Cidada.Application.Common;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Voting.Requests;
using Cobranca.Cidada.Application.Modules.Voting.Responses;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;
using Cobranca.Cidada.Domain.Modules.Voting.Aggregates;

namespace Cobranca.Cidada.Application.Modules.Voting.UseCases.SetVote;

/// <summary>Registra, altera ou desfaz o voto de uma pessoa. Nunca mexe na situação de execução do compromisso.</summary>
public class SetVoteUseCase : IUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly VotePolicy _votePolicy;
    private readonly VoteBurstGuard _burstGuard;

    public SetVoteUseCase(IUnitOfWork unitOfWork, VotePolicy votePolicy, VoteBurstGuard burstGuard)
    {
        _unitOfWork = unitOfWork;
        _votePolicy = votePolicy;
        _burstGuard = burstGuard;
    }

    public async Task<ErrorOr<VoteStateResponse>> ExecuteAsync(
        string voterKey,
        string slug,
        SetVoteRequest request,
        string? clientIp,
        CancellationToken cancellationToken = default)
    {
        var commitmentId = await _unitOfWork.CommitmentRepository.GetPublishedIdBySlugAsync(slug.Trim(), cancellationToken);
        if (commitmentId is null)
            return Error.NotFound("Commitment.NotFound", "Compromisso não encontrado.");

        var guard = await _burstGuard.CheckAsync(voterKey, clientIp, slug.Trim(), cancellationToken);
        if (guard.IsError)
            return guard.Errors;

        VotePolicy.TryParseRating(request.Rating, out var rating);
        var existing = await _unitOfWork.VoteRepository.GetAsync(voterKey, commitmentId.Value, cancellationToken);

        if (rating is null && !request.Charge)
        {
            if (existing is not null)
                _unitOfWork.VoteRepository.Remove(existing);
        }
        else if (existing is null)
        {
            await _unitOfWork.VoteRepository.CreateAsync(
                Vote.Create(voterKey, CommitmentId.Create(commitmentId.Value), rating, request.Charge), cancellationToken);
        }
        else
        {
            existing.Set(rating, request.Charge);
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        var tally = (await _unitOfWork.VoteRepository.GetTalliesAsync([commitmentId.Value], cancellationToken)).FirstOrDefault();
        var mine = (await _unitOfWork.VoteRepository.GetViewsAsync(voterKey, [commitmentId.Value], cancellationToken)).FirstOrDefault();

        return new VoteStateResponse(slug.Trim(), VotePolicy.ToMyVote(mine), _votePolicy.Summarize(tally));
    }
}
