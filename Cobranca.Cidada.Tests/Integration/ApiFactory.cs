using Microsoft.AspNetCore.Builder;
using Cobranca.Cidada.Infrastructure.Context;
using Microsoft.AspNetCore.Hosting;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Cobranca.Cidada.Domain.Modules.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Cobranca.Cidada.Tests.Integration;

/// <summary>Container PostgreSQL real compartilhado pela suíte de integração (nunca se mocka o banco).</summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder().WithImage("postgres:16-alpine").Build();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    /// <summary>Cria um banco novo e migrado, para isolar cada teste.</summary>
    public async Task<ApiFactory> CreateFactoryAsync(IDictionary<string, string?>? settings = null)
    {
        var databaseName = $"test_{Guid.NewGuid():N}";
        await using (var admin = new NpgsqlConnection(_container.GetConnectionString()))
        {
            await admin.OpenAsync();
            await using var command = admin.CreateCommand();
            command.CommandText = $"CREATE DATABASE \"{databaseName}\"";
            await command.ExecuteNonQueryAsync();
        }

        var connectionString = new NpgsqlConnectionStringBuilder(_container.GetConnectionString()) { Database = databaseName }.ConnectionString;
        var factory = new ApiFactory(connectionString, settings);

        using var scope = factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in UserRoles.All)
            await roles.CreateAsync(new IdentityRole(role));
        return factory;
    }
}

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly IDictionary<string, string?> _settings;

    public ApiFactory(string connectionString, IDictionary<string, string?>? settings = null)
    {
        _connectionString = connectionString;
        _settings = settings ?? new Dictionary<string, string?>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);
        builder.UseSetting("ConnectionStrings:Redis", "");
        foreach (var (key, value) in _settings)
            builder.UseSetting(key, value);

        builder.ConfigureServices(services =>
        {
            if (_settings.TryGetValue("ForwardedHeaders:Enabled", out var forwarded) && forwarded == "true")
                services.AddSingleton<Microsoft.AspNetCore.Hosting.IStartupFilter>(new LoopbackPeerStartupFilter());

            services.RemoveAll<IEmailService>();
            services.AddSingleton<CapturingEmailService>();
            services.AddSingleton<IEmailService>(sp => sp.GetRequiredService<CapturingEmailService>());

            services.RemoveAll<IGoogleIdentityVerifier>();
            services.AddSingleton<FakeGoogleIdentityVerifier>();
            services.AddSingleton<IGoogleIdentityVerifier>(sp => sp.GetRequiredService<FakeGoogleIdentityVerifier>());
        });
    }

    public CapturingEmailService Emails => Services.GetRequiredService<CapturingEmailService>();

    public FakeGoogleIdentityVerifier Google => Services.GetRequiredService<FakeGoogleIdentityVerifier>();
}

internal sealed class LoopbackPeerStartupFilter : Microsoft.AspNetCore.Hosting.IStartupFilter
{
    public Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> Configure(Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> next)
        => app =>
        {
            app.Use((context, nextMiddleware) =>
            {
                context.Connection.RemoteIpAddress ??= System.Net.IPAddress.Loopback;
                return nextMiddleware();
            });
            next(app);
        };
}

[CollectionDefinition(Name)]
public class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "postgres";
}

/// <summary>E-mail falso dos testes: guarda as mensagens para que o teste leia o código enviado.</summary>
public sealed class CapturingEmailService : IEmailService
{
    private readonly List<(string To, string Subject, string Body)> _sent = [];

    public IReadOnlyList<(string To, string Subject, string Body)> Sent
    {
        get { lock (_sent) return _sent.ToList(); }
    }

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        lock (_sent) _sent.Add((to, subject, htmlBody));
        return Task.CompletedTask;
    }

    /// <summary>Último código de 6 dígitos enviado ao e-mail informado.</summary>
    public string LastCodeFor(string email)
    {
        var message = Sent.Last(m => string.Equals(m.To, email, StringComparison.OrdinalIgnoreCase));
        var match = System.Text.RegularExpressions.Regex.Match(message.Body, @">(\d{6})<");
        Assert.True(match.Success, $"Nenhum código no e-mail '{message.Subject}'.");
        return match.Groups[1].Value;
    }
}

/// <summary>Verificador falso do Google: cada credencial de teste aponta para uma identidade registrada de antemão.</summary>
public sealed class FakeGoogleIdentityVerifier : IGoogleIdentityVerifier
{
    private readonly Dictionary<string, GoogleIdentity> _identities = [];

    public bool IsConfigured { get; set; } = true;

    /// <summary>Registra uma credencial e devolve a string a enviar no corpo da requisição.</summary>
    public string Issue(string subject, string email, string? name = "Pessoa Google", bool emailVerified = true, DateTimeOffset? issuedAt = null)
    {
        var credential = $"credencial-{Guid.NewGuid():N}";
        lock (_identities)
            _identities[credential] = new GoogleIdentity(subject, email, emailVerified, name, issuedAt ?? DateTimeOffset.UtcNow);
        return credential;
    }

    public Task<GoogleIdentity?> VerifyAsync(string credential, CancellationToken cancellationToken = default)
    {
        lock (_identities)
            return Task.FromResult(_identities.TryGetValue(credential, out var identity) ? identity : null);
    }
}
