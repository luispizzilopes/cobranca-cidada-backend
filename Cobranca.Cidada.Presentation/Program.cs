using Cobranca.Cidada.Presentation.Cli;
using Cobranca.Cidada.Presentation.Extensions;
using Serilog;

if (CliCommands.IsCliInvocation(args))
    return await CliCommands.RunAsync(args);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddConfigurations(builder.Configuration, builder.Environment);
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

var app = builder.Build();

app.AddRequestPipelineConfigurations();

app.Run();
return 0;

public partial class Program;
