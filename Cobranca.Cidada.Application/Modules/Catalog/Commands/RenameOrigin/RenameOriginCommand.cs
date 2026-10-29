using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Catalog.Commands.RenameOrigin;

public record RenameOriginCommand(string Code, string ActorId, string Name) : IRequest<ErrorOr<LookupResponse>>;
