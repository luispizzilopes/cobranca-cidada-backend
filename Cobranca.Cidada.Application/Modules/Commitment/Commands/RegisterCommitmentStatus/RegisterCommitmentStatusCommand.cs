using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Requests;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Commitment.Commands.RegisterCommitmentStatus;

public record RegisterCommitmentStatusCommand(string Slug, string ActorId, RegisterStatusRequest Request) : IRequest<ErrorOr<AdminCommitmentDetailResponse>>;
