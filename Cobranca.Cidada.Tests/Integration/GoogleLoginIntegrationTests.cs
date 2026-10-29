using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using Cobranca.Cidada.Application.Modules.Commitment.Commands.SeedCommitments;
using Cobranca.Cidada.Domain.Modules.User;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Cobranca.Cidada.Infrastructure.Context;
using Cobranca.Cidada.Tests.Application.Commitment;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cobranca.Cidada.Tests.Integration;

[Collection(PostgresCollection.Name)]
public class GoogleLoginIntegrationTests
{
    private const string Password = "Senh@-Muito-Forte-123";
    private const string NewPassword = "Outra-Senh@-Forte-456";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly PostgresFixture _postgres;

    public GoogleLoginIntegrationTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    private async Task<ApiFactory> NewFactoryAsync()
    {
        var factory = await _postgres.CreateFactoryAsync();
        using var scope = factory.Services.CreateScope();
        var seeded = await scope.ServiceProvider.GetRequiredService<IMediator>().Send(
            new SeedCommitmentsCommand(SeedFileLocator.Load(), false, new DateOnly(2027, 1, 5), new DateOnly(2030, 12, 31)));
        Assert.False(seeded.IsError);
        return factory;
    }

    private static async Task<LoginResponse> RegisterWithPasswordAsync(ApiFactory factory, HttpClient client, string email)
    {
        await client.PostAsJsonAsync("/api/v1/authentication/request-registration-code", new { email });
        var code = factory.Emails.LastCodeFor(email);
        var response = await client.PostAsJsonAsync("/api/v1/authentication/register",
            new { name = "Pessoa", email, password = Password, code, acceptedPrivacyPolicy = true });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResponse>(Json))!;
    }

    private static Task<HttpResponseMessage> GoogleAsync(HttpClient client, string credential, bool accepted = true)
        => client.PostAsJsonAsync("/api/v1/authentication/google", new { credential, acceptedPrivacyPolicy = accepted });

    private static async Task<LoginResponse> GoogleOkAsync(HttpClient client, string credential)
    {
        var response = await GoogleAsync(client, credential);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<LoginResponse>(Json))!;
    }

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.TryGetProperty("code", out var code) ? code.GetString() : null;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null) request.Content = JsonContent.Create(body);
        return request;
    }

    private static async Task<ProfileResponse> ProfileAsync(HttpClient client, string token)
    {
        var response = await client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/authentication/me", token));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProfileResponse>(Json))!;
    }

    private static async Task<HttpStatusCode> PasswordLoginStatusAsync(ApiFactory factory, string email, string password)
    {
        using var client = factory.CreateClient();
        return (await client.PostAsJsonAsync("/api/v1/authentication/login", new { email, password })).StatusCode;
    }

    private static async Task<int> UserCountAsync(ApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().Users.CountAsync();
    }

    [Fact]
    public async Task NewGoogleAccount_RequiresTheConsent_ThenCreatesAPasswordlessCitizen()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        var credential = factory.Google.Issue("google-sub-1", "nova@example.com", "Maria da Silva");

        var withoutConsent = await GoogleAsync(client, credential, accepted: false);
        Assert.Equal(HttpStatusCode.BadRequest, withoutConsent.StatusCode);
        Assert.Equal("Auth.ConsentRequired", await ErrorCodeAsync(withoutConsent));
        Assert.Equal(0, await UserCountAsync(factory));

        var login = await GoogleOkAsync(client, credential);
        Assert.Equal("nova@example.com", login.Email);
        Assert.Equal("Maria da Silva", login.Name);
        Assert.Contains(UserRoles.Citizen, login.Roles);

        var profile = await ProfileAsync(client, login.Token);
        Assert.False(profile.HasPassword);
        Assert.True(profile.GoogleLinked);
        Assert.NotNull(profile.ConsentAcceptedAtUtc);
        Assert.NotNull(profile.ConsentVersion);
    }

    [Fact]
    public async Task GoogleLogin_SetsTheRefreshCookie_AndTheSessionCanBeRefreshed()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient(new() { HandleCookies = true });
        var response = await GoogleAsync(client, factory.Google.Issue("google-sub-1", "nova@example.com"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"), c => c.Contains("HttpOnly", StringComparison.OrdinalIgnoreCase));

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync("/api/v1/authentication/refresh", null)).StatusCode);
    }

    [Fact]
    public async Task AGoogleAccountThatAlreadySignedIn_EntersTheSameUserAgain_WithoutNewConsent()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        var first = await GoogleOkAsync(client, factory.Google.Issue("google-sub-1", "nova@example.com"));
        var second = await GoogleOkAsync(client, factory.Google.Issue("google-sub-1", "nova@example.com"));

        Assert.Equal(first.UserId, second.UserId);
        Assert.Equal(1, await UserCountAsync(factory));
    }

    [Fact]
    public async Task GoogleLogin_WithAnEmailThatAlreadyHasAnAccount_LinksAndEntersTheSameUser()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        const string email = "conta@example.com";
        var byPassword = await RegisterWithPasswordAsync(factory, client, email);
        await client.SendAsync(Authorized(HttpMethod.Put, "/api/v1/commitments/ECON-001/vote", byPassword.Token, new { rating = "boa", charge = true }));

        var byGoogle = await GoogleAsync(client, factory.Google.Issue("google-sub-1", email), accepted: false);
        Assert.Equal(HttpStatusCode.OK, byGoogle.StatusCode);
        var google = (await byGoogle.Content.ReadFromJsonAsync<LoginResponse>(Json))!;

        Assert.Equal(byPassword.UserId, google.UserId);
        Assert.Equal(1, await UserCountAsync(factory));

        var votes = await client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/votes/mine", google.Token));
        Assert.Contains("ECON-001", await votes.Content.ReadAsStringAsync());

        var profile = await ProfileAsync(client, google.Token);
        Assert.True(profile.HasPassword);
        Assert.True(profile.GoogleLinked);
        Assert.Equal(HttpStatusCode.OK, await PasswordLoginStatusAsync(factory, email, Password));
    }

    [Fact]
    public async Task GoogleLogin_MatchesTheEmailIgnoringCase()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        var byPassword = await RegisterWithPasswordAsync(factory, client, "conta@example.com");

        var google = await GoogleOkAsync(client, factory.Google.Issue("google-sub-1", "Conta@Example.com"));

        Assert.Equal(byPassword.UserId, google.UserId);
        Assert.Equal(1, await UserCountAsync(factory));
    }

    [Fact]
    public async Task AfterLinking_TheNextGoogleLoginDoesNotDependOnTheEmailAnymore()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        var byPassword = await RegisterWithPasswordAsync(factory, client, "conta@example.com");
        await GoogleOkAsync(client, factory.Google.Issue("google-sub-1", "conta@example.com"));

        var again = await GoogleOkAsync(client, factory.Google.Issue("google-sub-1", "outro@example.com"));

        Assert.Equal(byPassword.UserId, again.UserId);
        Assert.Equal(1, await UserCountAsync(factory));
    }

    [Fact]
    public async Task GoogleLogin_RejectsAnEmailTheGoogleDidNotVerify_AndNeverLinksIt()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        const string email = "conta@example.com";
        await RegisterWithPasswordAsync(factory, client, email);

        var response = await GoogleAsync(client, factory.Google.Issue("google-sub-evil", email, emailVerified: false));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("Auth.GoogleEmailNotVerified", await ErrorCodeAsync(response));
        using var scope = factory.Services.CreateScope();
        Assert.Equal(0, await scope.ServiceProvider.GetRequiredService<AppDbContext>().Set<IdentityUserLogin<string>>().CountAsync());
    }

    [Fact]
    public async Task GoogleLogin_RejectsUnknownCredentials()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();

        var response = await GoogleAsync(client, "credencial-que-o-google-nao-emitiu");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, await UserCountAsync(factory));
    }

    [Fact]
    public async Task GoogleLogin_IsUnavailableWhenNoClientIdIsConfigured()
    {
        await using var factory = await NewFactoryAsync();
        factory.Google.IsConfigured = false;
        using var client = factory.CreateClient();

        var response = await GoogleAsync(client, factory.Google.Issue("google-sub-1", "nova@example.com"));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task AnAccountThatAlreadyHasAGoogleAccount_DoesNotAcceptAnotherOneWithTheSameEmail()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        const string email = "conta@example.com";
        await RegisterWithPasswordAsync(factory, client, email);
        await GoogleOkAsync(client, factory.Google.Issue("google-sub-1", email));

        var other = await GoogleAsync(client, factory.Google.Issue("google-sub-2", email));

        Assert.Equal(HttpStatusCode.Unauthorized, other.StatusCode);
        Assert.Equal("Auth.GoogleAccountMismatch", await ErrorCodeAsync(other));
    }

    [Fact]
    public async Task GoogleLogin_DoesNotEnterAnInactiveAccount()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        const string email = "conta@example.com";
        var account = await RegisterWithPasswordAsync(factory, client, email);
        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = (await users.FindByIdAsync(account.UserId))!;
            user.Active = false;
            await users.UpdateAsync(user);
        }

        var response = await GoogleAsync(client, factory.Google.Issue("google-sub-1", email));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task APasswordlessAccount_CannotUsePasswordLogin_NorChangePassword()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        const string email = "nova@example.com";
        var login = await GoogleOkAsync(client, factory.Google.Issue("google-sub-1", email));

        Assert.Equal(HttpStatusCode.Unauthorized, await PasswordLoginStatusAsync(factory, email, Password));

        var change = await client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/authentication/change-password", login.Token,
            new { currentPassword = Password, newPassword = NewPassword }));
        Assert.Equal(HttpStatusCode.BadRequest, change.StatusCode);
        Assert.Equal("Auth.NoPassword", await ErrorCodeAsync(change));
    }

    [Fact]
    public async Task APasswordlessAccount_CanCreateAPasswordByTheRecoveryFlow_AndThenEnterEitherWay()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        const string email = "nova@example.com";
        var login = await GoogleOkAsync(client, factory.Google.Issue("google-sub-1", email));

        await client.PostAsJsonAsync("/api/v1/authentication/request-password-reset", new { email });
        var code = factory.Emails.LastCodeFor(email);
        var reset = await client.PostAsJsonAsync("/api/v1/authentication/confirm-password-reset", new { email, code, newPassword = NewPassword });
        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);

        Assert.Equal(HttpStatusCode.OK, await PasswordLoginStatusAsync(factory, email, NewPassword));
        var again = await GoogleOkAsync(client, factory.Google.Issue("google-sub-1", email));
        Assert.Equal(login.UserId, again.UserId);
        Assert.True((await ProfileAsync(client, again.Token)).HasPassword);
    }

    [Fact]
    public async Task DeletingAPasswordlessAccount_NeedsAFreshCredentialFromTheLinkedGoogleAccount()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        var login = await GoogleOkAsync(client, factory.Google.Issue("google-sub-1", "nova@example.com"));
        await client.SendAsync(Authorized(HttpMethod.Put, "/api/v1/commitments/ECON-001/vote", login.Token, new { rating = "boa", charge = true }));

        var none = await client.SendAsync(Authorized(HttpMethod.Delete, "/api/v1/authentication/me", login.Token, new { }));
        Assert.Equal(HttpStatusCode.BadRequest, none.StatusCode);

        var someoneElse = await client.SendAsync(Authorized(HttpMethod.Delete, "/api/v1/authentication/me", login.Token,
            new { googleCredential = factory.Google.Issue("google-sub-2", "nova@example.com") }));
        Assert.Equal(HttpStatusCode.Unauthorized, someoneElse.StatusCode);

        var stale = await client.SendAsync(Authorized(HttpMethod.Delete, "/api/v1/authentication/me", login.Token,
            new { googleCredential = factory.Google.Issue("google-sub-1", "nova@example.com", issuedAt: DateTimeOffset.UtcNow.AddHours(-1)) }));
        Assert.Equal(HttpStatusCode.Unauthorized, stale.StatusCode);
        Assert.Equal(1, await UserCountAsync(factory));

        var deleted = await client.SendAsync(Authorized(HttpMethod.Delete, "/api/v1/authentication/me", login.Token,
            new { googleCredential = factory.Google.Issue("google-sub-1", "nova@example.com") }));
        Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);
        Assert.Equal(0, await UserCountAsync(factory));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(0, await db.Votes.CountAsync());
        Assert.Equal(0, await db.Set<IdentityUserLogin<string>>().CountAsync());
    }

    [Fact]
    public async Task AccountWithAPassword_StillNeedsThePassword_EvenWithAGoogleCredential()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        const string email = "conta@example.com";
        await RegisterWithPasswordAsync(factory, client, email);
        var google = await GoogleOkAsync(client, factory.Google.Issue("google-sub-1", email));

        var onlyGoogle = await client.SendAsync(Authorized(HttpMethod.Delete, "/api/v1/authentication/me", google.Token,
            new { googleCredential = factory.Google.Issue("google-sub-1", email) }));
        Assert.Equal(HttpStatusCode.Unauthorized, onlyGoogle.StatusCode);

        var withPassword = await client.SendAsync(Authorized(HttpMethod.Delete, "/api/v1/authentication/me", google.Token, new { password = Password }));
        Assert.Equal(HttpStatusCode.OK, withPassword.StatusCode);
    }

    [Fact]
    public async Task EmailChange_OfAPasswordlessAccount_IsConfirmedWithGoogle_AndGoogleLoginKeepsWorking()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        var login = await GoogleOkAsync(client, factory.Google.Issue("google-sub-1", "nova@example.com"));
        const string newEmail = "trocado@example.com";

        var request = await client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/authentication/request-email-change", login.Token,
            new { newEmail, googleCredential = factory.Google.Issue("google-sub-1", "nova@example.com") }));
        Assert.Equal(HttpStatusCode.OK, request.StatusCode);

        var code = factory.Emails.LastCodeFor(newEmail);
        var confirm = await client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/authentication/confirm-email-change", login.Token, new { newEmail, code }));
        Assert.Equal(HttpStatusCode.OK, confirm.StatusCode);

        var again = await GoogleOkAsync(client, factory.Google.Issue("google-sub-1", "nova@example.com"));
        Assert.Equal(login.UserId, again.UserId);
        Assert.Equal(newEmail, again.Email);
        Assert.Equal(1, await UserCountAsync(factory));
    }
}
