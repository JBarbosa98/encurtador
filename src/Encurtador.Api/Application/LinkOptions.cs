namespace Encurtador.Api.Application;

public sealed class LinkOptions
{
    public const string SectionName = "LinkOptions";

    public string BaseUrl { get; set; } = "http://localhost:5000";
}
