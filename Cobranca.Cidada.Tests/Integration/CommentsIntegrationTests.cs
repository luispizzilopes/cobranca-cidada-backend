using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using Cobranca.Cidada.Application.Modules.Comment.Responses;
using Cobranca.Cidada.Application.Modules.Commitment.Commands.SeedCommitments;
using Cobranca.Cidada.Application.Modules.Voting.Responses;
using Cobranca.Cidada.BuildingBlocks.Pagination;
using Cobranca.Cidada.Infrastructure.Context;
using Cobranca.Cidada.Tests.Application.Commitment;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cobranca.Cidada.Tests.Integration;

[Collection(PostgresCollection.Name)]
public class CommentsIntegrationTests
{
    private const string StrongPassword = "Senh@-Muito-Forte-123";
    private const string RecentUrl = "/api/v1/comments/recent";
    private const string CommentsUrl = "/api/v1/commitments/ECON-001/comments";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly PostgresFixture _postgres;

    public CommentsIntegrationTests(PostgresFixture postgres)
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

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string? token, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (token is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null) request.Content = JsonContent.Create(body);
        return request;
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<T>(Json))!;
    }

    private static async Task<CommentResponse> PostAsync(HttpClient client, string token, string text)
        => await ReadAsync<CommentResponse>(await client.SendAsync(Authorized(HttpMethod.Post, CommentsUrl, token, new { text })));

    private static async Task<CommentResponse> ReplyAsync(HttpClient client, string token, Guid parentId, string text)
        => await ReadAsync<CommentResponse>(await client.SendAsync(Authorized(HttpMethod.Post, CommentsUrl, token, new { text, parentId })));

    private static async Task<PaginedResult<CommentResponse>> ListAsync(HttpClient client, string? token = null)
        => await ReadAsync<PaginedResult<CommentResponse>>(await client.SendAsync(Authorized(HttpMethod.Get, CommentsUrl, token)));

    [Fact]
    public async Task Comment_RequiresVerifiedEmail_ButReadingIsPublic()
    {
        await using var factory = await NewSeededFactoryAsync();
        var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync(CommentsUrl, new { text = "Sem login" })).StatusCode);
        Assert.Empty((await ListAsync(client)).Items);

        var token = await VerifyAsVoterAsync(factory, client, "pessoa@example.com");
        var created = await PostAsync(client, token, "  Acho que o prazo é irreal.  ");
        Assert.Equal("Acho que o prazo é irreal.", created.Text);
        Assert.True(created.IsMine);

        var anonymous = await ListAsync(client);
        var item = Assert.Single(anonymous.Items);
        Assert.False(item.IsMine);
        Assert.Equal(1, anonymous.TotalCount);

        Assert.True(Assert.Single((await ListAsync(client, token)).Items).IsMine);
    }

    [Fact]
    public async Task RecentComments_ReturnLatestRootCommentPerCommitment_WithoutReplies()
    {
        await using var factory = await NewSeededFactoryAsync();
        var client = factory.CreateClient();
        var token = await VerifyAsVoterAsync(factory, client, "pessoa@example.com");

        Assert.Empty(await ReadAsync<List<RecentCommentResponse>>(await client.GetAsync(RecentUrl)));

        var first = await PostAsync(client, token, "Primeiro comentário em ECON-001");
        await PostAsync(client, token, "Segundo comentário em ECON-001");
        await ReplyAsync(client, token, first.Id, "Uma resposta que não deve aparecer");
        await ReadAsync<CommentResponse>(await client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/commitments/ECON-002/comments", token, new { text = "Comentário em ECON-002" })));

        var recent = await ReadAsync<List<RecentCommentResponse>>(await client.GetAsync(RecentUrl));
        Assert.Equal(2, recent.Count);
        Assert.Equal(["ECON-002", "ECON-001"], recent.Select(r => r.CommitmentId));
        Assert.Equal("Segundo comentário em ECON-001", recent[1].Text);
        Assert.All(recent, r => Assert.False(string.IsNullOrWhiteSpace(r.CommitmentTitle)));

        Assert.Single(await ReadAsync<List<RecentCommentResponse>>(await client.GetAsync($"{RecentUrl}?limit=1")));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync($"{RecentUrl}?limit=0")).StatusCode);
    }

    [Fact]
    public async Task Comment_ValidatesTextAndLimitsPerPerson()
    {
        await using var factory = await NewSeededFactoryAsync();
        var client = factory.CreateClient();
        var token = await VerifyAsVoterAsync(factory, client, "pessoa@example.com");

        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(Authorized(HttpMethod.Post, CommentsUrl, token, new { text = "  a " }))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(Authorized(HttpMethod.Post, CommentsUrl, token, new { text = new string('x', 1001) }))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/commitments/NAO-EXISTE/comments", token, new { text = "Olá, tudo bem" }))).StatusCode);

        for (var i = 0; i < 5; i++)
            await PostAsync(client, token, $"Comentário número {i}");

        var sixth = await client.SendAsync(Authorized(HttpMethod.Post, CommentsUrl, token, new { text = "Comentário excedente" }));
        Assert.Equal(HttpStatusCode.BadRequest, sixth.StatusCode);
    }

    [Fact]
    public async Task Comment_OnlyTheAuthorCanDeleteIt()
    {
        await using var factory = await NewSeededFactoryAsync();
        var client = factory.CreateClient();
        var author = await VerifyAsVoterAsync(factory, client, "autor@example.com");
        var other = await VerifyAsVoterAsync(factory, client, "outra@example.com");
        var created = await PostAsync(client, author, "Meu comentário");

        var url = $"/api/v1/comments/{created.Id}";
        Assert.Equal(HttpStatusCode.NotFound, (await client.SendAsync(Authorized(HttpMethod.Delete, url, other))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.SendAsync(Authorized(HttpMethod.Delete, url, null))).StatusCode);
        Assert.Single((await ListAsync(client)).Items);

        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(Authorized(HttpMethod.Delete, url, author))).StatusCode);
        Assert.Empty((await ListAsync(client)).Items);
    }

    [Fact]
    public async Task Comment_AdminCanModerate_ButVoterAndAnonymousCannot()
    {
        await using var factory = await NewSeededFactoryAsync();
        var client = factory.CreateClient();
        var voter = await VerifyAsVoterAsync(factory, client, "autor@example.com");
        var created = await PostAsync(client, voter, "Comentário a moderar");
        var url = $"/api/v1/admin/comments/{created.Id}";

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.SendAsync(Authorized(HttpMethod.Delete, url, null))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(Authorized(HttpMethod.Delete, url, voter))).StatusCode);

        const string adminEmail = "admin@example.com";
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/authentication/request-registration-code", new { email = adminEmail })).StatusCode);
        var code = factory.Emails.LastCodeFor(adminEmail);
        (await client.PostAsJsonAsync("/api/v1/authentication/register",
            new { name = "Admin", email = adminEmail, password = StrongPassword, code, acceptedPrivacyPolicy = true })).EnsureSuccessStatusCode();
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await context.Users.SingleAsync(u => u.Email == adminEmail)).IsAdmin = true;
            await context.SaveChangesAsync();
        }

        var login = await ReadAsync<LoginResponse>(await client.PostAsJsonAsync("/api/v1/authentication/login", new { email = adminEmail, password = StrongPassword }));
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(Authorized(HttpMethod.Delete, url, login.Token))).StatusCode);
        Assert.Empty((await ListAsync(client)).Items);
        Assert.Equal(HttpStatusCode.NotFound, (await client.SendAsync(Authorized(HttpMethod.Delete, url, login.Token))).StatusCode);
    }

    [Fact]
    public async Task Reply_IsNotLimited_AndDoesNotCountTowardsTheRootLimit()
    {
        await using var factory = await NewSeededFactoryAsync(new Dictionary<string, string?> { ["RateLimits:Comment"] = "1000" });
        var client = factory.CreateClient();
        var token = await VerifyAsVoterAsync(factory, client, "pessoa@example.com");
        var other = await VerifyAsVoterAsync(factory, client, "outra@example.com");

        var root = await PostAsync(client, other, "Comentário raiz de outra pessoa");
        for (var i = 0; i < 8; i++)
            Assert.Equal(root.Id, (await ReplyAsync(client, token, root.Id, $"Resposta {i}")).ParentId);

        for (var i = 0; i < 5; i++)
            await PostAsync(client, token, $"Comentário raiz {i}");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(Authorized(HttpMethod.Post, CommentsUrl, token, new { text = "Raiz excedente" }))).StatusCode);

        await ReplyAsync(client, token, root.Id, "Mais uma resposta");
    }

    [Fact]
    public async Task List_ReturnsOnlyRootsPaged_WithRepliesNested_AndReplyToReplyJoinsTheThread()
    {
        await using var factory = await NewSeededFactoryAsync();
        var client = factory.CreateClient();
        var token = await VerifyAsVoterAsync(factory, client, "pessoa@example.com");

        var root = await PostAsync(client, token, "Raiz");
        var reply = await ReplyAsync(client, token, root.Id, "Primeira resposta");
        var nested = await ReplyAsync(client, token, reply.Id, "Resposta da resposta");
        Assert.Equal(root.Id, nested.ParentId);

        var list = await ListAsync(client, token);
        Assert.Equal(1, list.TotalCount);
        var item = Assert.Single(list.Items);
        Assert.Null(item.ParentId);
        Assert.Equal(["Primeira resposta", "Resposta da resposta"], item.Replies.Select(r => r.Text));
        Assert.All(item.Replies, r => Assert.True(r.IsMine));
    }

    [Fact]
    public async Task Reply_RejectsUnknownParent_AndDeletingRootRemovesReplies()
    {
        await using var factory = await NewSeededFactoryAsync();
        var client = factory.CreateClient();
        var token = await VerifyAsVoterAsync(factory, client, "pessoa@example.com");

        var missing = await client.SendAsync(Authorized(HttpMethod.Post, CommentsUrl, token, new { text = "Órfã", parentId = Guid.NewGuid() }));
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);

        var root = await PostAsync(client, token, "Raiz");
        await ReplyAsync(client, token, root.Id, "Resposta");
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/comments/{root.Id}", token))).StatusCode);

        Assert.Empty((await ListAsync(client)).Items);
        using var scope = factory.Services.CreateScope();
        Assert.Equal(0, await scope.ServiceProvider.GetRequiredService<AppDbContext>().Comments.CountAsync());
    }
}
