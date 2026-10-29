using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using Cobranca.Cidada.Application.Modules.Commitment.Commands.SeedCommitments;
using Cobranca.Cidada.Application.Modules.Voting.Responses;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;
using Cobranca.Cidada.Domain.Modules.Voting.Aggregates;
using Cobranca.Cidada.Domain.Modules.Voting.Enums;
using Cobranca.Cidada.Infrastructure.Context;
using Cobranca.Cidada.Tests.Application.Commitment;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cobranca.Cidada.Tests.Integration;

[Collection(PostgresCollection.Name)]
public class AccountRecoveryIntegrationTests
{
    private const string Password = "Senh@-Muito-Forte-123";
    private const string NewPassword = "Outra-Senh@-Forte-456";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly PostgresFixture _postgres;

    public AccountRecoveryIntegrationTests(PostgresFixture postgres)
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

    private static async Task<LoginResponse> RegisterAsync(ApiFactory factory, HttpClient client, string email)
    {
        await client.PostAsJsonAsync("/api/v1/authentication/request-registration-code", new { email });
        var code = factory.Emails.LastCodeFor(email);
        var response = await client.PostAsJsonAsync("/api/v1/authentication/register",
            new { name = "Pessoa", email, password = Password, code, acceptedPrivacyPolicy = true });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResponse>(Json))!;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null) request.Content = JsonContent.Create(body);
        return request;
    }

    private static async Task<HttpStatusCode> LoginStatusAsync(ApiFactory factory, string email, string password)
    {
        using var client = factory.CreateClient();
        return (await client.PostAsJsonAsync("/api/v1/authentication/login", new { email, password })).StatusCode;
    }

    private static async Task<List<MyVoteItemResponse>> MyVotesAsync(HttpClient client, string token)
    {
        var response = await client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/votes/mine", token));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<MyVoteItemResponse>>(Json))!;
    }

    [Fact]
    public async Task PasswordReset_SetsNewPassword_RevokesSessions_AndNotifiesTheOwner()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        const string email = "conta@example.com";
        var account = await RegisterAsync(factory, client, email);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/authentication/request-password-reset", new { email })).StatusCode);
        var code = factory.Emails.LastCodeFor(email);

        var reset = await client.PostAsJsonAsync("/api/v1/authentication/confirm-password-reset", new { email, code, newPassword = NewPassword });
        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, await LoginStatusAsync(factory, email, Password));
        Assert.Equal(HttpStatusCode.OK, await LoginStatusAsync(factory, email, NewPassword));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/authentication/me", account.Token))).StatusCode);
        Assert.Contains(factory.Emails.Sent, m => m.To == email && m.Subject.Contains("senha foi alterada"));
    }

    [Fact]
    public async Task PasswordReset_CodeIsSingleUse_AndRejectsWeakPasswordsAndWrongCodes()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        const string email = "conta@example.com";
        await RegisterAsync(factory, client, email);
        await client.PostAsJsonAsync("/api/v1/authentication/request-password-reset", new { email });
        var code = factory.Emails.LastCodeFor(email);
        var wrong = code == "123456" ? "654321" : "123456";

        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/authentication/confirm-password-reset", new { email, code = wrong, newPassword = NewPassword })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/authentication/confirm-password-reset", new { email, code, newPassword = "fraca" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/authentication/confirm-password-reset", new { email, code, newPassword = NewPassword })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/authentication/confirm-password-reset", new { email, code, newPassword = "Mais-Uma-Senh@-789" })).StatusCode);
    }

    [Fact]
    public async Task PasswordReset_DoesNotRevealWhetherTheAccountExists()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        await RegisterAsync(factory, client, "conta@example.com");

        var existing = await client.PostAsJsonAsync("/api/v1/authentication/request-password-reset", new { email = "conta@example.com" });
        var unknown = await client.PostAsJsonAsync("/api/v1/authentication/request-password-reset", new { email = "ninguem@example.com" });

        Assert.Equal(HttpStatusCode.OK, existing.StatusCode);
        Assert.Equal(HttpStatusCode.OK, unknown.StatusCode);
        Assert.Equal(await existing.Content.ReadAsStringAsync(), await unknown.Content.ReadAsStringAsync());
        Assert.DoesNotContain(factory.Emails.Sent, m => m.To == "ninguem@example.com");
        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.PostAsJsonAsync("/api/v1/authentication/request-password-reset", new { email = "conta@example.com" })).StatusCode);
    }

    [Fact]
    public async Task PasswordReset_ClearsAnAccountLockout()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        const string email = "conta@example.com";
        await RegisterAsync(factory, client, email);
        for (var i = 0; i < 5; i++)
            await LoginStatusAsync(factory, email, "Errada-Errada-123!");
        Assert.Equal(HttpStatusCode.TooManyRequests, await LoginStatusAsync(factory, email, Password));

        await client.PostAsJsonAsync("/api/v1/authentication/request-password-reset", new { email });
        var code = factory.Emails.LastCodeFor(email);
        await client.PostAsJsonAsync("/api/v1/authentication/confirm-password-reset", new { email, code, newPassword = NewPassword });

        Assert.Equal(HttpStatusCode.OK, await LoginStatusAsync(factory, email, NewPassword));
    }

    [Fact]
    public async Task EmailChange_MovesTheVotesToTheNewEmail_RevokesSessionsAndNotifiesTheOldAddress()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        const string oldEmail = "antigo@example.com";
        const string newEmail = "novo@example.com";
        var account = await RegisterAsync(factory, client, oldEmail);
        await client.SendAsync(Authorized(HttpMethod.Put, "/api/v1/commitments/ECON-001/vote", account.Token, new { rating = "boa", charge = true }));
        await client.SendAsync(Authorized(HttpMethod.Put, "/api/v1/commitments/ECON-002/vote", account.Token, new { charge = true }));

        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/authentication/request-email-change", account.Token, new { newEmail, password = Password }))).StatusCode);
        var code = factory.Emails.LastCodeFor(newEmail);
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/authentication/confirm-email-change", account.Token, new { newEmail, code }))).StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/authentication/me", account.Token))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, await LoginStatusAsync(factory, oldEmail, Password));

        using var fresh = factory.CreateClient();
        var login = await fresh.PostAsJsonAsync("/api/v1/authentication/login", new { email = newEmail, password = Password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var token = (await login.Content.ReadFromJsonAsync<LoginResponse>(Json))!.Token;

        var mine = await MyVotesAsync(fresh, token);
        Assert.Equal(["ECON-001", "ECON-002"], mine.Select(v => v.CommitmentId).OrderBy(x => x).ToArray());

        using var scope = factory.Services.CreateScope();
        var oldKey = scope.ServiceProvider.GetRequiredService<IVoterKeyService>().Compute(oldEmail);
        Assert.Equal(0, await scope.ServiceProvider.GetRequiredService<AppDbContext>().Votes.CountAsync(v => v.VoterKey == oldKey));
        Assert.Contains(factory.Emails.Sent, m => m.To == oldEmail && m.Subject.Contains("alterado"));
    }

    [Fact]
    public async Task EmailChange_WhenBothMailboxesVotedOnTheSameCommitment_TheMostRecentVoteWins()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        const string oldEmail = "antigo@example.com";
        const string newEmail = "novo@example.com";
        var account = await RegisterAsync(factory, client, oldEmail);

        await client.SendAsync(Authorized(HttpMethod.Put, "/api/v1/commitments/ECON-001/vote", account.Token, new { rating = "boa", charge = false }));
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var newKey = scope.ServiceProvider.GetRequiredService<IVoterKeyService>().Compute(newEmail);
            var id = await db.Commitments.Where(c => c.Slug == "ECON-001").Select(c => c.Id).SingleAsync();
            await Task.Delay(50);
            db.Votes.Add(Vote.Create(newKey, CommitmentId.Create(id.Value), VoteRating.Bad, true));
            await db.SaveChangesAsync();
        }

        await client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/authentication/request-email-change", account.Token, new { newEmail, password = Password }));
        var code = factory.Emails.LastCodeFor(newEmail);
        var confirm = await client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/authentication/confirm-email-change", account.Token, new { newEmail, code }));
        Assert.Equal(HttpStatusCode.OK, confirm.StatusCode);

        using var check = factory.Services.CreateScope();
        var votes = await check.ServiceProvider.GetRequiredService<AppDbContext>().Votes.AsNoTracking().ToListAsync();
        var single = Assert.Single(votes);
        Assert.Equal(VoteRating.Bad, single.Rating);
        Assert.True(single.Charge);
    }

    [Fact]
    public async Task EmailChange_RequiresThePassword_AndADifferentMailbox()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        var account = await RegisterAsync(factory, client, "pessoa.um@gmail.com");

        var wrongPassword = await client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/authentication/request-email-change", account.Token, new { newEmail = "novo@example.com", password = "Errada-Errada-123!" }));
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);

        var alias = await client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/authentication/request-email-change", account.Token, new { newEmail = "pessoaum+x@gmail.com", password = Password }));
        Assert.Equal(HttpStatusCode.BadRequest, alias.StatusCode);

        var anonymous = await client.PostAsJsonAsync("/api/v1/authentication/request-email-change", new { newEmail = "novo@example.com", password = Password });
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
    }

    [Fact]
    public async Task EmailChange_ToAnAddressAlreadyInUse_LooksTheSameAndNeverSendsACode()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        var account = await RegisterAsync(factory, client, "a@example.com");
        using var other = factory.CreateClient();
        await RegisterAsync(factory, other, "b@example.com");

        var taken = await client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/authentication/request-email-change", account.Token, new { newEmail = "b@example.com", password = Password }));
        var free = await client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/authentication/request-email-change", account.Token, new { newEmail = "livre@example.com", password = Password }));

        Assert.Equal(HttpStatusCode.OK, taken.StatusCode);
        Assert.Equal(await taken.Content.ReadAsStringAsync(), await free.Content.ReadAsStringAsync());
        Assert.DoesNotMatch(@">\d{6}<", factory.Emails.Sent.Last(m => m.To == "b@example.com").Body);
    }

    [Fact]
    public async Task EmailChange_CodeBelongsToTheUserWhoRequestedIt()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        var first = await RegisterAsync(factory, client, "a@example.com");
        using var otherClient = factory.CreateClient();
        var second = await RegisterAsync(factory, otherClient, "b@example.com");

        await client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/authentication/request-email-change", first.Token, new { newEmail = "novo@example.com", password = Password }));
        var code = factory.Emails.LastCodeFor("novo@example.com");

        var stolen = await client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/authentication/confirm-email-change", second.Token, new { newEmail = "novo@example.com", code }));
        Assert.Equal(HttpStatusCode.BadRequest, stolen.StatusCode);
    }
}
