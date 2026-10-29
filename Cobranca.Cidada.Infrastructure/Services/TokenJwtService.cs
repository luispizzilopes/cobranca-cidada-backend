using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Cobranca.Cidada.Application.Interfaces.Services;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Cobranca.Cidada.Infrastructure.Services;

public class TokenJwtService : ITokenJwtService
{
    public const string TokenKindClaim = "typ";
    public const string UserKind = "user";
    public const string VoterKind = "voter";
    public const string VoterKeyClaim = "vk";
    public const string SessionClaim = "sid";
    public const string RoleClaim = "role";
    public const string AdminClaim = "admin";

    private readonly IConfiguration _configuration;
    private readonly TimeProvider _timeProvider;

    public TokenJwtService(IConfiguration configuration, TimeProvider timeProvider)
    {
        _configuration = configuration;
        _timeProvider = timeProvider;
    }

    public TokenJwtInformationResponse CreateTokenUser(AppUser user, Guid sessionId, string voterKey, IEnumerable<string> roles)
    {
        var expiration = _timeProvider.GetUtcNow().UtcDateTime.AddMinutes(
            double.Parse(_configuration["TokenConfiguration:AccessTokenExpireMinutes"] ?? "60"));

        var claims = new List<Claim>
        {
            new("Id", user.Id),
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, UnixNow(), ClaimValueTypes.Integer64),
            new(TokenKindClaim, UserKind),
            new(SessionClaim, sessionId.ToString()),
            new(VoterKeyClaim, voterKey),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.Name)
        };
        claims.AddRange(roles.Select(role => new Claim(RoleClaim, role)));
        if (user.IsAdmin)
            claims.Add(new Claim(AdminClaim, "true"));

        return Write(claims, expiration);
    }

    public TokenJwtInformationResponse CreateVoterToken(string voterKey)
    {
        var days = int.TryParse(_configuration["TokenConfiguration:VoterTokenExpireDays"], out var parsed) ? Math.Clamp(parsed, 1, 90) : 30;
        var expiration = _timeProvider.GetUtcNow().UtcDateTime.AddDays(days);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, UnixNow(), ClaimValueTypes.Integer64),
            new(TokenKindClaim, VoterKind),
            new(VoterKeyClaim, voterKey)
        };

        return Write(claims, expiration);
    }

    public string CreateRefreshToken()
        => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));

    public string HashRefreshToken(string refreshToken)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

    private TokenJwtInformationResponse Write(IEnumerable<Claim> claims, DateTime expiration)
    {
        var token = new JwtSecurityToken(
            issuer: _configuration["TokenConfiguration:Issuer"],
            audience: _configuration["TokenConfiguration:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:key"]!)),
                SecurityAlgorithms.HmacSha256));

        return new TokenJwtInformationResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            DateExpiration = expiration
        };
    }

    private string UnixNow() => _timeProvider.GetUtcNow().ToUnixTimeSeconds().ToString();
}
