using Cobranca.Cidada.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace Cobranca.Cidada.Tests.Infrastructure;

public class EmailAndVoterKeyTests
{
    private static VoterKeyService Service(string secret = "0123456789-0123456789-0123456789-ab")
        => new(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Voting:EmailHmacKey"] = secret }).Build());

    [Theory]
    [InlineData("Pessoa@Example.com", "pessoa@example.com")]
    [InlineData("  pessoa+vota@example.com ", "pessoa@example.com")]
    [InlineData("p.e.s.s.o.a@gmail.com", "pessoa@gmail.com")]
    [InlineData("pessoa+x@googlemail.com", "pessoa@gmail.com")]
    [InlineData("a.b@empresa.com.br", "a.b@empresa.com.br")]
    public void Canonicalize_CollapsesAliasesOfTheSameMailbox(string input, string expected)
        => Assert.Equal(expected, EmailNormalizer.Canonicalize(input));

    [Fact]
    public void VoterKey_IsStableForAliasesOfTheSameMailbox()
    {
        var service = Service();
        Assert.Equal(service.Compute("Pessoa.Um+a@gmail.com"), service.Compute("pessoaum@gmail.com"));
    }

    [Fact]
    public void VoterKey_DiffersByPersonAndBySecret_AndDoesNotContainTheEmail()
    {
        var key = Service().Compute("a@example.com");

        Assert.NotEqual(key, Service().Compute("b@example.com"));
        Assert.NotEqual(key, Service("OUTRA-CHAVE-OUTRA-CHAVE-OUTRA-CHAVE-123").Compute("a@example.com"));
        Assert.Equal(64, key.Length);
        Assert.DoesNotContain("example", key);
    }

    [Fact]
    public void VoterKeyService_RejectsWeakSecret()
        => Assert.Throws<InvalidOperationException>(() => Service("curta"));
}
