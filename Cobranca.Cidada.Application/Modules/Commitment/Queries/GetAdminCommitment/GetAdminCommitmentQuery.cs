using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Commitment.Queries.GetAdminCommitment;

public record GetAdminCommitmentQuery(string Slug) : IRequest<ErrorOr<AdminCommitmentDetailResponse>>;
