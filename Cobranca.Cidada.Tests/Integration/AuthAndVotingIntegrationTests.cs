using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Application.Modules.Voting.Responses;
using Cobranca.Cidada.BuildingBlocks.Pagination;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Microsoft.AspNetCore.Identity;
using Cobranca.Cidada.Domain.Modules.Voting.Aggregates;
using Cobranca.Cidada.Domain.Modules.Voting.Enums;
using Cobranca.Cidada.Infrastructure.Context;
using Cobranca.Cidada.Application.Modules.Commitment.Commands.SeedCommitments;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Cobranca.Cidada.Tests.Application.Commitment;

namespace Cobranca.Cidada.Tests.Integration;

[Collection(PostgresCollection.Name)]
public class AuthAndVotingIntegrationTests
{
    private const string StrongPassword = "Senh@-Muito-Forte-123";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly PostgresFixture _postgres;

    public AuthAndVotingIntegrationTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    private async Task<ApiFactory> NewSeededFactoryAsync(IDictionary<string, string?>? settings = null)
    {
        var factory = await _postgres.CreateFactoryAsync(settings);
        using var scope = factory.Services.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<IMediator>().Send(
            new SeedCommitmentsCommand(SeedFileLocator.Load(), false, new DateOnly(2027, 1, 5), new DateOnly(2030, 12, 31)));
        Assert.False(result.IsError);
        return factory;
    }

    private static async Task<string> VerifyAsVoterAsync(ApiFactory factory, HttpClient client, string email)
    {
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/voter/code", new { email })).StatusCode);
        var code = factory.Emails.LastCodeFor(email);
        var response = await client.PostAsJsonAsync("/api/v1/voter/verify", new { email, code });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<VoterTokenResponse>(Json))!.Token;
    }

    private static async Task<LoginResponse> RegisterAsync(ApiFactory factory, HttpClient client, string email, string name = "Pessoa Teste")
    {
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/authentication/request-registration-code", new { email })).StatusCode);
        var code = factory.Emails.LastCodeFor(email);
        var response = await client.PostAsJsonAsync("/api/v1/authentication/register",
            new { name, email, password = StrongPassword, code, acceptedPrivacyPolicy = true });
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

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(Json))!;
    }

    [Fact]
    public async Task Voter_CanVoteWithOnlyAVerifiedEmail_WithoutCreatingAnAccount()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();

        var token = await VerifyAsVoterAsync(factory, client, "visitante@example.com");

        var voted = await ReadAsync<VoteStateResponse>(await client.SendAsync(
            Authorized(HttpMethod.Put, "/api/v1/commitments/ECON-001/vote", token, new { rating = "boa", charge = true })));
        Assert.Equal("boa", voted.MyVote!.Rating);
        Assert.True(voted.MyVote.Charge);
        Assert.Equal(1, voted.Votes.Charges);

        var detail = await ReadAsync<CommitmentDetailResponse>(await client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/commitments/ECON-001", token)));
        Assert.Equal("boa", detail.MyVote!.Rating);
        Assert.Equal(1, detail.Votes.Charges);

        var anonymous = await ReadAsync<CommitmentDetailResponse>(await client.GetAsync("/api/v1/commitments/ECON-001"));
        Assert.Null(anonymous.MyVote);

        using var scope = factory.Services.CreateScope();
        Assert.Equal(0, await scope.ServiceProvider.GetRequiredService<AppDbContext>().Users.CountAsync());
    }

    [Fact]
    public async Task Vote_CanBeChangedAndUndone()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        var token = await VerifyAsVoterAsync(factory, client, "visitante@example.com");

        await client.SendAsync(Authorized(HttpMethod.Put, "/api/v1/commitments/ECON-001/vote", token, new { rating = "boa", charge = true }));
        var changed = await ReadAsync<VoteStateResponse>(await client.SendAsync(
            Authorized(HttpMethod.Put, "/api/v1/commitments/ECON-001/vote", token, new { rating = "ma", charge = false })));
        Assert.Equal("ma", changed.MyVote!.Rating);

        var mine = await ReadAsync<List<MyVoteItemResponse>>(await client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/votes/mine", token)));
        var single = Assert.Single(mine);
        Assert.Equal("ECON-001", single.CommitmentId);

        var undone = await ReadAsync<VoteStateResponse>(await client.SendAsync(Authorized(HttpMethod.Delete, "/api/v1/commitments/ECON-001/vote", token)));
        Assert.Null(undone.MyVote);
        Assert.Empty(await ReadAsync<List<MyVoteItemResponse>>(await client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/votes/mine", token))));
    }

    [Fact]
    public async Task Vote_DoesNotChangeTheExecutionStatus()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        var token = await VerifyAsVoterAsync(factory, client, "visitante@example.com");
        var before = await ReadAsync<CommitmentDetailResponse>(await client.GetAsync("/api/v1/commitments/ECON-006"));

        await client.SendAsync(Authorized(HttpMethod.Put, "/api/v1/commitments/ECON-006/vote", token, new { rating = "ma", charge = true }));

        var after = await ReadAsync<CommitmentDetailResponse>(await client.GetAsync("/api/v1/commitments/ECON-006"));
        Assert.Equal(before.Status, after.Status);
        Assert.Equal(before.StatusHistory.Count, after.StatusHistory.Count);
    }

    [Fact]
    public async Task Vote_RequiresVerifiedEmail_AndRejectsInvalidInput()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();

        var anonymous = await client.PutAsJsonAsync("/api/v1/commitments/ECON-001/vote", new { rating = "boa", charge = true });
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        var forged = await client.SendAsync(Authorized(HttpMethod.Put, "/api/v1/commitments/ECON-001/vote", "token.forjado.invalido", new { charge = true }));
        Assert.Equal(HttpStatusCode.Unauthorized, forged.StatusCode);

        var token = await VerifyAsVoterAsync(factory, client, "visitante@example.com");
        var badRating = await client.SendAsync(Authorized(HttpMethod.Put, "/api/v1/commitments/ECON-001/vote", token, new { rating = "otimo", charge = false }));
        Assert.Equal(HttpStatusCode.BadRequest, badRating.StatusCode);

        var missing = await client.SendAsync(Authorized(HttpMethod.Put, "/api/v1/commitments/NAO-EXISTE/vote", token, new { charge = true }));
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task VoterCode_WrongCodeIsRejected_AndLocksAfterFiveAttempts()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        const string email = "visitante@example.com";
        await client.PostAsJsonAsync("/api/v1/voter/code", new { email });
        var realCode = factory.Emails.LastCodeFor(email);
        var wrong = realCode == "000000" ? "111111" : "000000";

        for (var i = 0; i < 4; i++)
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/voter/verify", new { email, code = wrong })).StatusCode);

        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.PostAsJsonAsync("/api/v1/voter/verify", new { email, code = wrong })).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.PostAsJsonAsync("/api/v1/voter/verify", new { email, code = realCode })).StatusCode);
    }

    [Fact]
    public async Task VoterCode_CannotBeRequestedAgainWithinTheCooldown()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/voter/code", new { email = "visitante@example.com" })).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.PostAsJsonAsync("/api/v1/voter/code", new { email = "visitante@example.com" })).StatusCode);
    }

    [Fact]
    public async Task EmailAliasesOfTheSameMailbox_AreTheSameVoter()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();

        string key;
        using (var scope = factory.Services.CreateScope())
        {
            key = scope.ServiceProvider.GetRequiredService<IVoterKeyService>().Compute("pessoaum@gmail.com");
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var id = await db.Commitments.Where(c => c.Slug == "ECON-001").Select(c => c.Id).SingleAsync();
            db.Votes.Add(Vote.Create(key, CommitmentId.Create(id.Value), VoteRating.Good, true));
            await db.SaveChangesAsync();
        }

        var token = await VerifyAsVoterAsync(factory, client, "Pessoa.Um+vota@gmail.com");
        var mine = await ReadAsync<List<MyVoteItemResponse>>(await client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/votes/mine", token)));
        Assert.Equal("ECON-001", Assert.Single(mine).CommitmentId);

        await client.SendAsync(Authorized(HttpMethod.Put, "/api/v1/commitments/ECON-001/vote", token, new { rating = "ma", charge = true }));
        using var check = factory.Services.CreateScope();
        Assert.Equal(1, await check.ServiceProvider.GetRequiredService<AppDbContext>().Votes.CountAsync());
    }

    [Fact]
    public async Task VoterToken_CannotUseAccountEndpoints()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        var token = await VerifyAsVoterAsync(factory, client, "visitante@example.com");

        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/authentication/me", token))).StatusCode);
    }

    [Fact]
    public async Task DeletingVotes_ByAVoterWithoutAccount_AlsoRevokesTheToken()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        var token = await VerifyAsVoterAsync(factory, client, "visitante@example.com");
        await client.SendAsync(Authorized(HttpMethod.Put, "/api/v1/commitments/ECON-001/vote", token, new { charge = true }));

        var deleted = await ReadAsync<DeleteVotesResponse>(await client.SendAsync(Authorized(HttpMethod.Delete, "/api/v1/votes/mine", token)));
        Assert.Equal(1, deleted.DeletedVotes);

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/votes/mine", token))).StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(0, await db.Votes.CountAsync());
        Assert.Equal(0, await db.VoterIdentities.CountAsync());
    }

    [Fact]
    public async Task Account_RegisterRequiresCodeStrongPasswordAndConsent()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        const string email = "conta@example.com";
        await client.PostAsJsonAsync("/api/v1/authentication/request-registration-code", new { email });
        var code = factory.Emails.LastCodeFor(email);

        var weak = await client.PostAsJsonAsync("/api/v1/authentication/register", new { name = "X", email, password = "fraca", code, acceptedPrivacyPolicy = true });
        Assert.Equal(HttpStatusCode.BadRequest, weak.StatusCode);

        var noConsent = await client.PostAsJsonAsync("/api/v1/authentication/register", new { name = "X", email, password = StrongPassword, code, acceptedPrivacyPolicy = false });
        Assert.Equal(HttpStatusCode.BadRequest, noConsent.StatusCode);

        var wrongCode = await client.PostAsJsonAsync("/api/v1/authentication/register", new { name = "X", email, password = StrongPassword, code = code == "123456" ? "654321" : "123456", acceptedPrivacyPolicy = true });
        Assert.Equal(HttpStatusCode.BadRequest, wrongCode.StatusCode);

        var ok = await client.PostAsJsonAsync("/api/v1/authentication/register", new { name = "Pessoa", email, password = StrongPassword, code, acceptedPrivacyPolicy = true });
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
    }

    [Fact]
    public async Task Account_RegistersLogsInRefreshesAndLogsOut()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        const string email = "conta@example.com";

        var registered = await RegisterAsync(factory, client, email);
        Assert.Contains("Citizen", registered.Roles);

        var profile = await ReadAsync<ProfileResponse>(await client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/authentication/me", registered.Token)));
        Assert.Equal(email, profile.Email);
        Assert.NotNull(profile.ConsentAcceptedAtUtc);
        Assert.False(string.IsNullOrEmpty(profile.ConsentVersion));

        using var other = factory.CreateClient();
        var wrongPassword = await other.PostAsJsonAsync("/api/v1/authentication/login", new { email, password = "Errada-Errada-123!" });
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);

        var login = await ReadAsync<LoginResponse>(await other.PostAsJsonAsync("/api/v1/authentication/login", new { email, password = StrongPassword }));
        var refreshed = await ReadAsync<LoginResponse>(await other.PostAsync("/api/v1/authentication/refresh", null));
        Assert.NotEqual(login.Token, refreshed.Token);

        Assert.Equal(HttpStatusCode.OK, (await other.PostAsync("/api/v1/authentication/logout", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await other.PostAsync("/api/v1/authentication/refresh", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await other.SendAsync(Authorized(HttpMethod.Get, "/api/v1/authentication/me", refreshed.Token))).StatusCode);
    }

    [Fact]
    public async Task Login_IsBlockedForThisIpAndAccount_AfterRepeatedWrongPasswords()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        const string email = "conta@example.com";
        await RegisterAsync(factory, client, email);
        using var other = factory.CreateClient();

        for (var i = 0; i < 5; i++)
            await other.PostAsJsonAsync("/api/v1/authentication/login", new { email, password = "Errada-Errada-123!" });

        var locked = await other.PostAsJsonAsync("/api/v1/authentication/login", new { email, password = StrongPassword });
        Assert.Equal(HttpStatusCode.TooManyRequests, locked.StatusCode);
        Assert.True(locked.Headers.Contains("Retry-After"));
    }

    [Fact]
    public async Task FailedLogin_LooksTheSame_ForUnknownEmail_WrongPassword_AndInactiveAccount()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        await RegisterAsync(factory, client, "conta@example.com");
        await RegisterAsync(factory, client, "inativa@example.com");
        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Cobranca.Cidada.Domain.Modules.User.Entities.AppUser>>();
            var inactive = (await users.FindByEmailAsync("inativa@example.com"))!;
            inactive.Active = false;
            await users.UpdateAsync(inactive);
        }

        var bodies = new List<string>();
        foreach (var (email, password) in new[] { ("ninguem@example.com", StrongPassword), ("conta@example.com", "Errada-Errada-123!"), ("inativa@example.com", StrongPassword) })
        {
            var response = await client.PostAsJsonAsync("/api/v1/authentication/login", new { email, password });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            bodies.Add(await response.Content.ReadAsStringAsync());
        }

        Assert.Single(bodies.Distinct());
    }

    [Fact]
    public async Task LoginBlock_AppliesEquallyToUnknownEmails_SoItRevealsNothing()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();

        for (var i = 0; i < 5; i++)
            await client.PostAsJsonAsync("/api/v1/authentication/login", new { email = "ninguem@example.com", password = "Errada-Errada-123!" });

        var blocked = await client.PostAsJsonAsync("/api/v1/authentication/login", new { email = "ninguem@example.com", password = "Errada-Errada-123!" });
        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
    }

    [Fact]
    public async Task LoginBlock_FromOneIp_DoesNotLockTheOwnerOutFromAnotherIp()
    {
        await using var factory = await NewSeededFactoryAsync(new Dictionary<string, string?>
        {
            ["ForwardedHeaders:Enabled"] = "true",
            ["ForwardedHeaders:KnownProxies:0"] = "127.0.0.1"
        });
        using var client = factory.CreateClient();
        const string email = "conta@example.com";
        await RegisterAsync(factory, client, email);

        async Task<HttpStatusCode> LoginFromAsync(string ip, string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/authentication/login") { Content = JsonContent.Create(new { email, password }) };
            request.Headers.Add("X-Forwarded-For", ip);
            return (await client.SendAsync(request)).StatusCode;
        }

        for (var i = 0; i < 5; i++)
            await LoginFromAsync("203.0.113.7", "Errada-Errada-123!");

        Assert.Equal(HttpStatusCode.TooManyRequests, await LoginFromAsync("203.0.113.7", StrongPassword));
        Assert.Equal(HttpStatusCode.OK, await LoginFromAsync("198.51.100.20", StrongPassword));
    }

    [Fact]
    public async Task RegistrationCode_ForAnExistingAccount_DoesNotRevealIt()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var created = await users.CreateAsync(new AppUser { Name = "Existente", Email = "conta@example.com", UserName = "conta@example.com", EmailConfirmed = true }, StrongPassword);
            Assert.True(created.Succeeded);
        }

        var existing = await client.PostAsJsonAsync("/api/v1/authentication/request-registration-code", new { email = "conta@example.com" });
        var fresh = await client.PostAsJsonAsync("/api/v1/authentication/request-registration-code", new { email = "nova@example.com" });

        Assert.Equal(HttpStatusCode.OK, existing.StatusCode);
        Assert.Equal(HttpStatusCode.OK, fresh.StatusCode);
        Assert.Equal(await existing.Content.ReadAsStringAsync(), await fresh.Content.ReadAsStringAsync());

        Assert.Contains("já tem conta", factory.Emails.Sent.Single(m => m.To == "conta@example.com").Subject);
        Assert.Matches(@">\d{6}<", factory.Emails.Sent.Single(m => m.To == "nova@example.com").Body);
    }

    [Fact]
    public async Task Account_VoteIsUnifiedWithTheVotesMadeBeforeWithOnlyTheEmail()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        const string email = "pessoa@example.com";

        var voterToken = await VerifyAsVoterAsync(factory, client, email);
        await client.SendAsync(Authorized(HttpMethod.Put, "/api/v1/commitments/ECON-001/vote", voterToken, new { rating = "boa", charge = true }));

        var account = await RegisterAsync(factory, client, email);
        var mine = await ReadAsync<List<MyVoteItemResponse>>(await client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/votes/mine", account.Token)));

        Assert.Equal("ECON-001", Assert.Single(mine).CommitmentId);
    }

    [Fact]
    public async Task Account_PasswordChange_ChecksCurrentPassword_AndRevokesSessions()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        const string email = "troca-senha@example.com";
        const string newPassword = "Outr@-Senha-Forte-456";
        var account = await RegisterAsync(factory, client, email);

        var wrong = await client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/authentication/change-password", account.Token,
            new { currentPassword = "Errada-Errada-123!", newPassword }));
        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);

        var weak = await client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/authentication/change-password", account.Token,
            new { currentPassword = StrongPassword, newPassword = "fraca" }));
        Assert.Equal(HttpStatusCode.BadRequest, weak.StatusCode);

        var same = await client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/authentication/change-password", account.Token,
            new { currentPassword = StrongPassword, newPassword = StrongPassword }));
        Assert.Equal(HttpStatusCode.BadRequest, same.StatusCode);

        var changed = await client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/authentication/change-password", account.Token,
            new { currentPassword = StrongPassword, newPassword }));
        Assert.Equal(HttpStatusCode.OK, changed.StatusCode);

        var oldLogin = await client.PostAsJsonAsync("/api/v1/authentication/login", new { email, password = StrongPassword });
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);
        var newLogin = await client.PostAsJsonAsync("/api/v1/authentication/login", new { email, password = newPassword });
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }

    [Fact]
    public async Task Account_DeletionRemovesUserSessionsAndVotes()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        const string email = "conta@example.com";
        var account = await RegisterAsync(factory, client, email);
        await client.SendAsync(Authorized(HttpMethod.Put, "/api/v1/commitments/ECON-001/vote", account.Token, new { charge = true }));

        var wrong = await client.SendAsync(Authorized(HttpMethod.Delete, "/api/v1/authentication/me", account.Token, new { password = "Errada-Errada-123!" }));
        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);

        var deleted = await client.SendAsync(Authorized(HttpMethod.Delete, "/api/v1/authentication/me", account.Token, new { password = StrongPassword }));
        Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(0, await db.Users.CountAsync());
        Assert.Equal(0, await db.UserSessions.CountAsync());
        Assert.Equal(0, await db.Votes.CountAsync());
    }

    [Fact]
    public async Task PublicCounts_AreAlwaysShown()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        var below = await InsertVotesAsync(factory, "ECON-001", 19, charge: true);
        var hidden = await ReadAsync<CommitmentDetailResponse>(await client.GetAsync("/api/v1/commitments/ECON-001"));
        Assert.Equal(19, hidden.Votes.Charges);
        Assert.False(hidden.Votes.ChargesHidden);

        await InsertVotesAsync(factory, "ECON-001", 1, charge: true, offset: below);
        var shown = await ReadAsync<CommitmentDetailResponse>(await client.GetAsync("/api/v1/commitments/ECON-001"));
        Assert.Equal(20, shown.Votes.Charges);
        Assert.False(shown.Votes.ChargesHidden);
    }

    [Fact]
    public async Task MyCharges_FilterRequiresAToken_AndListsOnlyWhatThePersonCharged()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        var token = await VerifyAsVoterAsync(factory, client, "visitante@example.com");
        await client.SendAsync(Authorized(HttpMethod.Put, "/api/v1/commitments/ECON-001/vote", token, new { charge = true }));
        await client.SendAsync(Authorized(HttpMethod.Put, "/api/v1/commitments/ECON-002/vote", token, new { rating = "boa", charge = false }));

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/commitments?myCharges=true")).StatusCode);

        var mine = await ReadAsync<PaginedResult<CommitmentSummaryResponse>>(await client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/commitments?myCharges=true", token)));
        Assert.Equal("ECON-001", Assert.Single(mine.Items).Id);
        Assert.True(mine.Items.Single().MyVote!.Charge);
    }

    [Fact]
    public async Task Sorting_ByMostChargedAndByRating()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();
        await InsertVotesAsync(factory, "SAUDE-001", 3, charge: true);
        await InsertVotesAsync(factory, "ECON-003", 2, charge: true, offset: 100);
        await InsertVotesAsync(factory, "EDU-001", 4, charge: false, rating: VoteRating.Good, offset: 200);
        await InsertVotesAsync(factory, "AGRO-001", 5, charge: false, rating: VoteRating.Bad, offset: 300);

        var charged = await ReadAsync<PaginedResult<CommitmentSummaryResponse>>(await client.GetAsync("/api/v1/commitments?sort=mais-cobradas&pageSize=2"));
        Assert.Equal(["SAUDE-001", "ECON-003"], charged.Items.Select(i => i.Id).ToArray());

        var best = await ReadAsync<PaginedResult<CommitmentSummaryResponse>>(await client.GetAsync("/api/v1/commitments?sort=melhor-avaliadas&pageSize=1"));
        Assert.Equal("EDU-001", best.Items.Single().Id);

        var worst = await ReadAsync<PaginedResult<CommitmentSummaryResponse>>(await client.GetAsync("/api/v1/commitments?sort=pior-avaliadas&pageSize=1"));
        Assert.Equal("AGRO-001", worst.Items.Single().Id);
    }

    [Fact]
    public async Task Catalog_ExposesTheVotingDisclaimerAndMinimum()
    {
        await using var factory = await NewSeededFactoryAsync();
        using var client = factory.CreateClient();

        var catalog = await ReadAsync<Cobranca.Cidada.Application.Modules.Catalog.Responses.CatalogResponse>(await client.GetAsync("/api/v1/catalog"));

        Assert.Equal(20, catalog.Voting.MinimumPublicVotes);
        Assert.Contains("Não é pesquisa estatística", catalog.Voting.Disclaimer);
    }

    /// <summary>Insere votos de pessoas fictícias diretamente no banco (chaves sintéticas), para testar contagens.</summary>
    private static async Task<int> InsertVotesAsync(ApiFactory factory, string slug, int count, bool charge, VoteRating? rating = null, int offset = 0)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var id = await db.Commitments.Where(c => c.Slug == slug).Select(c => c.Id).SingleAsync();
        for (var i = 0; i < count; i++)
            db.Votes.Add(Vote.Create((offset + i).ToString("x").PadLeft(64, 'f'), CommitmentId.Create(id.Value), rating, charge));
        await db.SaveChangesAsync();
        return count;
    }
}
