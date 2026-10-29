using ErrorOr;
using Cobranca.Cidada.Application.Interfaces.Services;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Voting.Requests;
using Cobranca.Cidada.Application.Modules.Voting.Responses;
using Cobranca.Cidada.Application.Modules.Voting.UseCases.RequestVoterCode;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.Voting.Entities;

namespace Cobranca.Cidada.Application.Modules.Voting.UseCases.VerifyVoterCode;

public class VerifyVoterCodeUseCase : IUseCase
{
    private readonly IVerificationCodeStore _codeStore;
    private readonly IVoterKeyService _voterKeyService;
    private readonly ITokenJwtService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyVoterCodeUseCase(
        IVerificationCodeStore codeStore,
        IVoterKeyService voterKeyService,
        ITokenJwtService tokenService,
        IUnitOfWork unitOfWork)
    {
        _codeStore = codeStore;
        _voterKeyService = voterKeyService;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<VoterTokenResponse>> ExecuteAsync(VerifyVoterCodeRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();

        var outcome = await _codeStore.VerifyAsync(RequestVoterCodeUseCase.Purpose, email, request.Code.Trim(), cancellationToken);
        if (outcome != VerificationOutcome.Valid)
        {
            return outcome switch
            {
                VerificationOutcome.Expired => Error.Validation("Voter.CodeExpired", "Código expirado ou inexistente. Solicite um novo código."),
                VerificationOutcome.TooManyAttempts => Error.Custom(429, "Voter.TooManyAttempts", "Muitas tentativas. Aguarde alguns minutos e tente novamente."),
                _ => Error.Validation("Voter.InvalidCode", "Código inválido.")
            };
        }

        var voterKey = _voterKeyService.Compute(email);
        var identity = await _unitOfWork.VoterIdentityRepository.GetAsync(voterKey, cancellationToken);
        if (identity is null)
            await _unitOfWork.VoterIdentityRepository.CreateAsync(VoterIdentity.Create(voterKey), cancellationToken);
        else
            identity.MarkVerified();
        await _unitOfWork.CommitAsync(cancellationToken);

        var token = _tokenService.CreateVoterToken(voterKey);
        return new VoterTokenResponse(token.Token, token.DateExpiration);
    }
}
