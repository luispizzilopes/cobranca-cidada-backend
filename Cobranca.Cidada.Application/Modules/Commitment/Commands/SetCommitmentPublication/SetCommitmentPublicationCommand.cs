using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Commitment.Commands.SetCommitmentPublication;

public record SetCommitmentPublicationCommand(string Slug, string ActorId, bool Published) : IRequest<ErrorOr<AdminCommitmentDetailResponse>>;
