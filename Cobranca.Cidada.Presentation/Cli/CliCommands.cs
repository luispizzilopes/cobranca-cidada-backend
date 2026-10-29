using System.Text.Json;
using Cobranca.Cidada.Application.Modules.Commitment.Commands.SeedCommitments;
using Cobranca.Cidada.Application.Modules.Commitment.Requests;
using Cobranca.Cidada.Infrastructure.Context;
using Cobranca.Cidada.Presentation.Extensions;
using MediatR;
using Cobranca.Cidada.Domain.Modules.User;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cobranca.Cidada.Presentation.Cli;

/// <summary>Subcomandos de linha de comando: <c>migrate</c>, <c>seed</c> e <c>set-admin</c>. Executam e encerram, sem subir o servidor HTTP.</summary>
public static class CliCommands
{
    public static bool IsCliInvocation(string[] args)
        => args.Length > 0 && args[0] is "seed" or "migrate" or "set-admin";

    public static async Task<int> RunAsync(string[] args)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
        builder.Services.AddConfigurations(builder.Configuration, builder.Environment);

        await using var app = builder.Build();
        await using var scope = app.Services.CreateAsyncScope();

        return args[0] switch
        {
            "migrate" => await MigrateAsync(scope.ServiceProvider),
            "set-admin" => await SetAdminAsync(scope.ServiceProvider, args.Skip(1).ToArray()),
            _ => await SeedAsync(scope.ServiceProvider, args.Skip(1).ToArray())
        };
    }

    private static async Task<int> MigrateAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in UserRoles.All.Where(role => !roles.RoleExistsAsync(role).GetAwaiter().GetResult()))
            await roles.CreateAsync(new IdentityRole(role));

        Console.WriteLine("Migrações aplicadas.");
        return 0;
    }

    /// <summary>Concede (ou, com <c>--revoke</c>, retira) a flag IsAdmin de uma conta existente. Vale a partir da próxima requisição ao painel.</summary>
    private static async Task<int> SetAdminAsync(IServiceProvider services, string[] args)
    {
        var email = ReadOption(args, "--email");
        if (string.IsNullOrWhiteSpace(email))
        {
            Console.Error.WriteLine("Uso: set-admin --email <e-mail da conta> [--revoke]");
            return 2;
        }

        var users = services.GetRequiredService<UserManager<AppUser>>();
        var user = await users.FindByEmailAsync(email.Trim());
        if (user is null)
        {
            Console.Error.WriteLine("Nenhuma conta com esse e-mail. A pessoa precisa se cadastrar antes.");
            return 1;
        }

        user.IsAdmin = !args.Contains("--revoke");
        var result = await users.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                Console.Error.WriteLine($"Erro: {error.Description}");
            return 1;
        }

        Console.WriteLine(user.IsAdmin ? "Conta marcada como administradora." : "Flag de administrador removida da conta.");
        return 0;
    }

    private static async Task<int> SeedAsync(IServiceProvider services, string[] args)
    {
        var file = ReadOption(args, "--file");
        if (string.IsNullOrWhiteSpace(file))
        {
            Console.Error.WriteLine("Uso: seed --file <promessas.json> [--overwrite] [--mandate-start yyyy-MM-dd] [--mandate-end yyyy-MM-dd]");
            return 2;
        }

        if (!File.Exists(file))
        {
            Console.Error.WriteLine($"Arquivo não encontrado: {file}");
            return 2;
        }

        SeedFileRequest? request;
        await using (var stream = File.OpenRead(file))
        {
            request = await JsonSerializer.DeserializeAsync<SeedFileRequest>(stream);
        }

        if (request is null)
        {
            Console.Error.WriteLine("Arquivo de carga vazio ou inválido.");
            return 2;
        }

        var start = DateOnly.Parse(ReadOption(args, "--mandate-start") ?? "2027-01-05");
        var end = DateOnly.Parse(ReadOption(args, "--mandate-end") ?? "2030-12-31");
        var overwrite = args.Contains("--overwrite");

        var result = await services.GetRequiredService<IMediator>().Send(new SeedCommitmentsCommand(request, overwrite, start, end));

        if (result.IsError)
        {
            foreach (var error in result.Errors)
                Console.Error.WriteLine($"Erro: {error.Description}");
            return 1;
        }

        var report = result.Value;
        Console.WriteLine($"Total: {report.Total} | Criados: {report.Created} | Atualizados: {report.Overwritten} | Ignorados: {report.Skipped} | Rejeitados: {report.Rejected.Count}");
        Console.WriteLine($"Áreas novas: {report.AreasCreated} | Origens novas: {report.OriginsCreated}");
        foreach (var rejection in report.Rejected)
            Console.WriteLine($"  REJEITADO {rejection.Id}: {string.Join(" | ", rejection.Reasons)}");

        return report.Rejected.Count > 0 ? 1 : 0;
    }

    private static string? ReadOption(string[] args, string name)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == name && i + 1 < args.Length) return args[i + 1];
            if (args[i].StartsWith(name + "=", StringComparison.Ordinal)) return args[i][(name.Length + 1)..];
        }

        return null;
    }
}
