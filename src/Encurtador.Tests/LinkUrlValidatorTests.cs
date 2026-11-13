using Encurtador.Api.Application;

namespace Encurtador.Tests;

public class LinkUrlValidatorTests
{
    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com/abc?x=1")]
    public void IsValid_AcceptsHttpAndHttpsUrls(string url)
    {
        Assert.True(LinkUrlValidator.IsValid(url));
    }

    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("example.com")]
    [InlineData("")]
    public void IsValid_RejectsInvalidUrls(string url)
    {
        Assert.False(LinkUrlValidator.IsValid(url));
    }
}
