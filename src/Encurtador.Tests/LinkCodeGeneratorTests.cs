using Encurtador.Api.Application;

namespace Encurtador.Tests;

public class LinkCodeGeneratorTests
{
    [Fact]
    public void Generate_ReturnsSevenCharactersUsingAllowedAlphabet()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        var code = LinkCodeGenerator.Generate();

        Assert.Equal(7, code.Length);
        Assert.All(code, character => Assert.Contains(character, alphabet));
    }
}
