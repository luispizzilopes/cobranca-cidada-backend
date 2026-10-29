using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Cobranca.Cidada.Application.Interfaces.Services;
using Cobranca.Cidada.Application.Modules.Commitment.Commands.SeedCommitments;
using Cobranca.Cidada.Domain.Modules.Audit;
using Cobranca.Cidada.Domain.Modules.Voting.Entities;
using Cobranca.Cidada.Infrastructure.Context;
using Cobranca.Cidada.Tests.Application.Commitment;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cobranca.Cidada.Tests.Integration;

[Collection(PostgresCollection.Name)]
public class VoteBurstIntegrationTests
{
    private readonly PostgresFixture _postgres;

    public VoteBurstIntegrationTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    private async Task<ApiFactory> NewFactoryAsync(Dictionary<string, string?>? settings = null)
    {
        settings ??= new Dictionary<string, string?>();
        settings["ForwardedHeaders:Enabled"] = "true";
        settings["ForwardedHeaders:KnownProxies:0"] = "127.0.0.1";

        var factory = await _postgres.CreateFactoryAsync(settings);
        using var scope = factory.Services.CreateScope();
        var seeded = await scope.ServiceProvider.GetRequiredService<IMediator>().Send(
            new SeedCommitmentsCommand(SeedFileLocator.Load(), false, new DateOnly(2027, 1, 5), new DateOnly(2030, 12, 31)));
        Assert.False(seeded.IsError);
        return factory;
    }

    /// <summary>Cria uma pessoa de teste com e-mail "verificado" e devolve o token (sem passar pelo envio de código).</summary>
    private static async Task<string> NewVoterTokenAsync(ApiFactory factory, string label)
    {
        using var scope = factory.Services.CreateScope();
        var key = label.PadLeft(64, 'e');
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.VoterIdentities.Add(VoterIdentity.Create(key));
        await db.SaveChangesAsync();
        return scope.ServiceProvider.GetRequiredService<ITokenJwtService>().CreateVoterToken(key).Token;
    }

    private static async Task<HttpResponseMessage> VoteAsync(HttpClient client, string token, string slug, string? ip = null, bool charge = true)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/commitments/{slug}/vote") { Content = JsonContent.Create(new { charge }) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (ip is not null) request.Headers.Add("X-Forwarded-For", ip);
        return await client.SendAsync(request);
    }

    private static string[] Slugs(int count) => Enumerable.Range(1, count).Select(i => $"ECON-{i:000}").ToArray();

    [Fact]
    public async Task OneVoterFiringTooManyVotesInASecondsIsThrottled_WithRetryAfter()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        var token = await NewVoterTokenAsync(factory, "a1");

        foreach (var slug in Slugs(10))
            Assert.Equal(HttpStatusCode.OK, (await VoteAsync(client, token, slug)).StatusCode);

        var blocked = await VoteAsync(client, token, "ECON-011");
        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
        Assert.True(blocked.Headers.TryGetValues("Retry-After", out var retry));
        Assert.InRange(int.Parse(retry!.Single()), 1, 300);
        Assert.Contains("Vote.BurstDetected", await blocked.Content.ReadAsStringAsync());

        await Task.Delay(TimeSpan.FromSeconds(11));
        Assert.Equal(HttpStatusCode.TooManyRequests, (await VoteAsync(client, token, "ECON-001")).StatusCode);

        var other = await NewVoterTokenAsync(factory, "b2");
        Assert.Equal(HttpStatusCode.OK, (await VoteAsync(client, other, "ECON-001")).StatusCode);
    }

    [Fact]
    public async Task ABlockedVote_IsNotRecorded_AndTheBlockIsAudited_WithoutPersonalData()
    {
        await using var factory = await NewFactoryAsync(new() { ["Voting:Burst:VoterMaxActions"] = "2" });
        using var client = factory.CreateClient();
        var token = await NewVoterTokenAsync(factory, "a1");

        await VoteAsync(client, token, "ECON-001");
        await VoteAsync(client, token, "ECON-002");
        Assert.Equal(HttpStatusCode.TooManyRequests, (await VoteAsync(client, token, "ECON-003")).StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(2, await db.Votes.CountAsync());
        var audit = await db.AuditLogs.SingleAsync(a => a.Action == AuditActions.VoteBurstBlocked);
        Assert.Equal("system", audit.Actor);
        Assert.DoesNotContain("e".PadLeft(64, 'e'), audit.Details ?? string.Empty);
        Assert.DoesNotContain("203.0.113", audit.Details ?? string.Empty);
    }

    [Fact]
    public async Task ManyVotersFromTheSameIp_AreBlockedAtThatIp_ButNotOtherIps()
    {
        await using var factory = await NewFactoryAsync(new() { ["Voting:Burst:IpMaxDistinctVoters"] = "3" });
        using var client = factory.CreateClient();
        const string ip = "203.0.113.7";

        for (var i = 0; i < 3; i++)
        {
            var token = await NewVoterTokenAsync(factory, $"c{i}");
            Assert.Equal(HttpStatusCode.OK, (await VoteAsync(client, token, "ECON-001", ip)).StatusCode);
        }

        var fourth = await NewVoterTokenAsync(factory, "c3");
        Assert.Equal(HttpStatusCode.TooManyRequests, (await VoteAsync(client, fourth, "ECON-001", ip)).StatusCode);

        var fifth = await NewVoterTokenAsync(factory, "c4");
        Assert.Equal(HttpStatusCode.TooManyRequests, (await VoteAsync(client, fifth, "ECON-002", ip)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await VoteAsync(client, fifth, "ECON-002", "198.51.100.9")).StatusCode);
    }

    [Fact]
    public async Task TooManyActionsFromOneIpInAFewSeconds_AreBlocked()
    {
        await using var factory = await NewFactoryAsync(new() { ["Voting:Burst:IpMaxActions"] = "4" });
        using var client = factory.CreateClient();

        for (var i = 0; i < 4; i++)
        {
            var token = await NewVoterTokenAsync(factory, $"d{i}");
            Assert.Equal(HttpStatusCode.OK, (await VoteAsync(client, token, $"ECON-00{i + 1}", "203.0.113.50")).StatusCode);
        }

        var last = await NewVoterTokenAsync(factory, "d9");
        Assert.Equal(HttpStatusCode.TooManyRequests, (await VoteAsync(client, last, "ECON-005", "203.0.113.50")).StatusCode);
    }

    [Fact]
    public async Task ASpikeOnOneCommitment_RaisesOneAlertForCurators_WithoutBlockingAnyone()
    {
        await using var factory = await NewFactoryAsync(new() { ["Voting:Burst:CommitmentSpikeMaxActions"] = "3" });
        using var client = factory.CreateClient();

        for (var i = 0; i < 6; i++)
        {
            var token = await NewVoterTokenAsync(factory, $"f{i}");
            Assert.Equal(HttpStatusCode.OK, (await VoteAsync(client, token, "ECON-001", $"198.51.100.{i + 1}")).StatusCode);
        }

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(6, await db.Votes.CountAsync());
        var alert = await db.AuditLogs.SingleAsync(a => a.Action == AuditActions.VoteBurstOnCommitment);
        Assert.Equal("ECON-001", alert.EntityId);
    }

    [Fact]
    public async Task NormalUse_IsNeverThrottled()
    {
        await using var factory = await NewFactoryAsync();
        using var client = factory.CreateClient();
        var token = await NewVoterTokenAsync(factory, "g1");

        Assert.Equal(HttpStatusCode.OK, (await VoteAsync(client, token, "ECON-001", "203.0.113.9")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await VoteAsync(client, token, "ECON-001", "203.0.113.9", charge: false)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await VoteAsync(client, token, "ECON-002", "203.0.113.9")).StatusCode);
    }
}
