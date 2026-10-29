using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cobranca.Cidada.Domain.Interfaces.Repositories.User;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Voting;
using Cobranca.Cidada.Domain.Modules.User;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Cobranca.Cidada.Infrastructure.Context;
using Cobranca.Cidada.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Cobranca.Cidada.CrossCutting.IoC.Infrastructure.Identity;

public static class IdentityDependencyInjection
{
    public const string AccountPolicy = "Account";
    public const string VoterPolicy = "Voter";
    public const string AdminPolicy = "Admin";

    public static void AddIdentityDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequiredLength = 12;
            options.Password.RequiredUniqueChars = 4;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.User.RequireUniqueEmail = true;
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                ValidIssuer = configuration["TokenConfiguration:Issuer"],
                ValidAudience = configuration["TokenConfiguration:Audience"],
                RoleClaimType = TokenJwtService.RoleClaim,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:key"]!))
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = ValidateTokenAsync
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AccountPolicy, policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(TokenJwtService.TokenKindClaim, TokenJwtService.UserKind));

            options.AddPolicy(VoterPolicy, policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(TokenJwtService.VoterKeyClaim));

            options.AddPolicy(AdminPolicy, policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(TokenJwtService.TokenKindClaim, TokenJwtService.UserKind)
                .RequireClaim(TokenJwtService.AdminClaim, "true"));
        });
    }

    /// <summary>
    /// A claim "admin" vive no token por até uma hora. Para que revogar a flag no banco valha na hora,
    /// quem apresenta a claim é conferido contra o banco e perde os privilégios se não for mais admin (ou estiver inativo).
    /// </summary>
    private static async Task DropStaleAdminClaimsAsync(ClaimsPrincipal principal, string userId, IServiceProvider services, CancellationToken ct)
    {
        if (principal.FindFirst(TokenJwtService.AdminClaim) is null || principal.Identity is not ClaimsIdentity identity)
            return;

        var stillAdmin = await services.GetRequiredService<AppDbContext>().Users
            .AsNoTracking()
            .AnyAsync(user => user.Id == userId && user.IsAdmin && user.Active, ct);
        if (stillAdmin)
            return;

        foreach (var claim in identity.Claims
                     .Where(c => c.Type == TokenJwtService.AdminClaim
                                 || (c.Type == TokenJwtService.RoleClaim && c.Value == UserRoles.Admin))
                     .ToList())
            identity.RemoveClaim(claim);
    }

    private static async Task ValidateTokenAsync(TokenValidatedContext context)
    {
        var principal = context.Principal;
        var kind = principal?.FindFirst(TokenJwtService.TokenKindClaim)?.Value;
        var services = context.HttpContext.RequestServices;
        var ct = context.HttpContext.RequestAborted;

        switch (kind)
        {
            case TokenJwtService.UserKind:
            {
                var userId = principal!.FindFirst("Id")?.Value ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                var sessionValue = principal.FindFirst(TokenJwtService.SessionClaim)?.Value;
                if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(sessionValue, out var sessionId))
                {
                    context.Fail("Sessão ausente no token.");
                    return;
                }

                var sessions = services.GetRequiredService<IUserSessionRepository>();
                if (!await sessions.IsActiveAsync(sessionId, userId, ct))
                {
                    context.Fail("Sessão revogada ou expirada.");
                    return;
                }

                await DropStaleAdminClaimsAsync(principal, userId, services, ct);
                return;
            }
            case TokenJwtService.VoterKind:
            {
                var voterKey = principal!.FindFirst(TokenJwtService.VoterKeyClaim)?.Value;
                var issuedAt = long.TryParse(principal.FindFirst(JwtRegisteredClaimNames.Iat)?.Value, out var iat)
                    ? DateTimeOffset.FromUnixTimeSeconds(iat).UtcDateTime
                    : (DateTime?)null;
                if (string.IsNullOrWhiteSpace(voterKey) || issuedAt is null)
                {
                    context.Fail("Token de votante inválido.");
                    return;
                }

                var identities = services.GetRequiredService<IVoterIdentityRepository>();
                if (!await identities.IsTokenValidAsync(voterKey, issuedAt.Value, ct))
                    context.Fail("Verificação de e-mail revogada ou inexistente.");
                return;
            }
            default:
                context.Fail("Tipo de token desconhecido.");
                return;
        }
    }
}
