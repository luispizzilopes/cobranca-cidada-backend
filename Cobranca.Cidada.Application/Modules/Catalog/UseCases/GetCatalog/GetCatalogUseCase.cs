using ErrorOr;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Catalog.Responses;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.Commitment.Enums;
using Cobranca.Cidada.Application.Modules.Commitment.Mappers;
using Cobranca.Cidada.Application.Modules.Voting.Policies;

namespace Cobranca.Cidada.Application.Modules.Catalog.UseCases.GetCatalog;

public class GetCatalogUseCase : IUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly VotePolicy _votePolicy;

    public GetCatalogUseCase(IUnitOfWork unitOfWork, VotePolicy votePolicy)
    {
        _unitOfWork = unitOfWork;
        _votePolicy = votePolicy;
    }

    public async Task<ErrorOr<CatalogResponse>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var areas = await _unitOfWork.AreaRepository.GetAllAsync(cancellationToken);
        var origins = await _unitOfWork.OriginRepository.GetAllAsync(cancellationToken);

        return new CatalogResponse(
            areas.Select(a => new LookupResponse(a.Code, a.Name)).ToList(),
            origins.Select(o => new LookupResponse(o.Code, o.Name)).ToList(),
            Enum.GetValues<CommitmentStatus>().Select(s => s.ToResponse()).ToList(),
            new VotingInfoResponse(_votePolicy.MinimumPublicVotes, VotePolicy.Disclaimer));
    }
}
