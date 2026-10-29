using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Commitment.Queries.GetCommitmentBySlug;

public record GetCommitmentBySlugQuery(string Slug, string? VoterKey = null) : IRequest<ErrorOr<CommitmentDetailResponse>>;
