using ErrorOr;
using Cobranca.Cidada.Application.Common;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;

namespace Cobranca.Cidada.Application.Modules.Commitment.UseCases.GetAdminCommitment;

public class GetAdminCommitmentUseCase : IUseCase
{
    private readonly AdminCommitmentReader _reader;

    public GetAdminCommitmentUseCase(AdminCommitmentReader reader)
    {
        _reader = reader;
    }

    public Task<ErrorOr<AdminCommitmentDetailResponse>> ExecuteAsync(string slug, CancellationToken cancellationToken = default)
        => _reader.GetAsync(slug, cancellationToken);
}
