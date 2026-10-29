using Cobranca.Cidada.Application.Modules.Commitment.Services;

namespace Cobranca.Cidada.Tests.Application.Commitment;

public class SeedCommitmentMapperTests
{
    [Theory]
    [InlineData("LULA26-ECON-006", "ECON-006")]
    [InlineData("lula26-saude-001", "saude-001")]
    [InlineData("OUTRO-1", "OUTRO-1")]
    [InlineData(null, null)]
    [InlineData("  ", null)]
    public void SlugFromLegacyId_StripsPrefix(string? id, string? expected)
        => Assert.Equal(expected, SeedCommitmentMapper.SlugFromLegacyId(id));

    [Theory]
    [InlineData("https://www.tse.jus.br/eleicoes/arquivo", "primary")]
    [InlineData("https://www.gov.br/planalto/x", "primary")]
    [InlineData("https://www1.folha.uol.com.br/x", "press")]
    [InlineData("nao-e-url", "press")]
    public void ClassifySource_UsesOfficialDomains(string url, string expected)
        => Assert.Equal(expected, SeedCommitmentMapper.ClassifySource(url));
}
