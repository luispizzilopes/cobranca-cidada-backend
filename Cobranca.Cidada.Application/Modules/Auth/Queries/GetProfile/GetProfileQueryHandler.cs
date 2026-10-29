using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using Cobranca.Cidada.Application.Modules.Auth.UseCases.GetProfile;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, ErrorOr<ProfileResponse>>
{
    private readonly GetProfileUseCase _useCase;

    public GetProfileQueryHandler(GetProfileUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<ProfileResponse>> Handle(GetProfileQuery query, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(query.UserId);
}
