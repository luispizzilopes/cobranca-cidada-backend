using Cobranca.Cidada.Application.Modules.Comment.Filters;
using Xunit;

namespace Cobranca.Cidada.Tests.Application;

public class ProfanityCensorTests
{
    private static readonly string[] Words = ["merda", "cu", "foda se", "filho da puta", "shit", "puta", "ass", "porra"];

    [Theory]
    [InlineData("isso é uma merda", "isso é uma m****")]
    [InlineData("MERDA!", "M****!")]
    [InlineData("Mérda total", "M**** total")]
    [InlineData("que merdas", "que merdas")]
    [InlineData("vai se foder, filho da puta", "vai se foder, f**** d* p***")]
    [InlineData("Foda-se isso", "F***-s* isso")]
    [InlineData("cuidado com o cu", "cuidado com o c*")]
    [InlineData("Bullshit e shit.", "Bullshit e s***.")]
    [InlineData("texto limpo", "texto limpo")]
    [InlineData("putaaaaa merda", "p******* m****")]
    [InlineData("MERRRRDA", "M*******")]
    [InlineData("shiiiit", "s******")]
    [InlineData("cuuuu", "c****")]
    [InlineData("cuidado, cuuuidado", "cuidado, cuuuidado")]
    public void Mascara_palavra_inteira_mantendo_a_primeira_letra(string input, string expected)
        => Assert.Equal(expected, ProfanityCensor.Censor(input, Words));

    [Fact]
    public void Sem_palavras_devolve_o_texto_como_veio()
        => Assert.Equal("merda", ProfanityCensor.Censor("merda", []));

    [Theory]
    [InlineData("as casas são boas", "as casas são boas")]
    [InlineData("assss", "a****")]
    [InlineData("pora", "pora")]
    [InlineData("porrrrra", "p*******")]
    [InlineData("fooooda se", "f****** s*")]
    public void Repeticao_so_vale_para_letras_a_mais(string input, string expected)
        => Assert.Equal(expected, ProfanityCensor.Censor(input, Words));
}
