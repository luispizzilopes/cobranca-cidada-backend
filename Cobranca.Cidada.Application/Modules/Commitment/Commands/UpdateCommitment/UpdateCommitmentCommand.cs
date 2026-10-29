using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Requests;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Commitment.Commands.UpdateCommitment;

public record UpdateCommitmentCommand(string Slug, string ActorId, UpdateCommitmentRequest Request) : IRequest<ErrorOr<AdminCommitmentDetailResponse>>;
