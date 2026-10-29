using Cobranca.Cidada.Application.Modules.Auth.Responses;
using Cobranca.Cidada.Domain.Modules.User.Entities;

namespace Cobranca.Cidada.Application.Interfaces.Services;

public interface ITokenJwtService
{
    /// <summary>Token de conta: carrega o id do usuário, a sessão, os papéis e a chave pseudônima de votante.</summary>
    TokenJwtInformationResponse CreateTokenUser(AppUser user, Guid sessionId, string voterKey, IEnumerable<string> roles);

    /// <summary>Token de votante com e-mail verificado e sem conta: só permite votar e gerir os próprios votos.</summary>
    TokenJwtInformationResponse CreateVoterToken(string voterKey);

    string CreateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
