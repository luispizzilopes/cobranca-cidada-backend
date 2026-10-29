using Cobranca.Cidada.Presentation.Extensions;
using Microsoft.Extensions.Configuration;

namespace Cobranca.Cidada.Tests.Infrastructure;

public class ForwardedHeadersOptionsTests
{
    private static IConfiguration Config(params (string Key, string? Value)[] values)
        => new ConfigurationBuilder().AddInMemoryCollection(values.ToDictionary(v => v.Key, v => v.Value)).Build();

    [Fact]
    public void Disabled_ReturnsNull()
        => Assert.Null(RequestPipelineConfigurationExtension.BuildForwardedHeadersOptions(Config()));

    [Fact]
    public void Enabled_WithoutTrustedProxies_RefusesToStart()
        => Assert.Throws<InvalidOperationException>(() =>
            RequestPipelineConfigurationExtension.BuildForwardedHeadersOptions(Config(("ForwardedHeaders:Enabled", "true"))));

    [Fact]
    public void Enabled_WithProxiesAndNetworks_TrustsOnlyThose()
    {
        var options = RequestPipelineConfigurationExtension.BuildForwardedHeadersOptions(Config(
            ("ForwardedHeaders:Enabled", "true"),
            ("ForwardedHeaders:KnownProxies:0", "10.0.0.5"),
            ("ForwardedHeaders:KnownNetworks:0", "172.16.0.0/12")))!;

        Assert.Equal([System.Net.IPAddress.Parse("10.0.0.5")], options.KnownProxies);
        Assert.Single(options.KnownNetworks);
        Assert.Equal(1, options.ForwardLimit);
    }

    [Fact]
    public void Enabled_WithInvalidAddress_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => RequestPipelineConfigurationExtension.BuildForwardedHeadersOptions(Config(
            ("ForwardedHeaders:Enabled", "true"), ("ForwardedHeaders:KnownProxies:0", "nao-e-ip"))));
        Assert.Throws<InvalidOperationException>(() => RequestPipelineConfigurationExtension.BuildForwardedHeadersOptions(Config(
            ("ForwardedHeaders:Enabled", "true"), ("ForwardedHeaders:KnownNetworks:0", "10.0.0.0"))));
    }
}
