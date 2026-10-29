using System.Security.Cryptography;
using ErrorOr;
using Cobranca.Cidada.Application.Common.Emails;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Voting.Requests;
using Cobranca.Cidada.Domain.Interfaces.Services;

namespace Cobranca.Cidada.Application.Modules.Voting.UseCases.RequestVoterCode;

public class RequestVoterCodeUseCase : IUseCase
{
    public const string Purpose = "voter";

    private readonly IEmailService _emailService;
    private readonly IVerificationCodeStore _codeStore;

    public RequestVoterCodeUseCase(IEmailService emailService, IVerificationCodeStore codeStore)
    {
        _emailService = emailService;
        _codeStore = codeStore;
    }

    public async Task<ErrorOr<Success>> ExecuteAsync(RequestVoterCodeRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();

        if (!await _codeStore.TryReserveSendAsync(Purpose, email, cancellationToken))
            return Error.Custom(429, "Voter.TooManyRequests", "Aguarde um minuto antes de pedir um novo código.");

        try
        {
            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            await _codeStore.StoreAsync(Purpose, email, code, cancellationToken);
            await _emailService.SendAsync(email, EmailTemplates.Subjects.VoterCode, EmailTemplates.VoterCode(code), cancellationToken);
        }
        catch
        {
            await _codeStore.RemoveAsync(Purpose, email, cancellationToken);
            return Error.Failure("Voter.EmailFailed", "Não foi possível enviar o e-mail de verificação.");
        }

        return Result.Success;
    }
}
