using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Requests;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Commitment.Commands.CreateCommitment;

public record CreateCommitmentCommand(string ActorId, CreateCommitmentRequest Request) : IRequest<ErrorOr<AdminCommitmentDetailResponse>>;
