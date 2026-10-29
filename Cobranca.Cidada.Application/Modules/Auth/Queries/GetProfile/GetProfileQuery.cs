using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Queries.GetProfile;

public record GetProfileQuery(string UserId) : IRequest<ErrorOr<ProfileResponse>>;
