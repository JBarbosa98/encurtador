using Encurtador.Api.Application;
using Encurtador.Api.Domain;
using Encurtador.Api.Infrastructure.Repositories;

namespace Encurtador.Tests;

public class LinkServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesLinkWithGeneratedCodeAndManagementKey()
    {
        var repository = new InMemoryLinkRepository();
        var service = CreateService(repository);

        var result = await service.CreateAsync("https://example.com", null, null);

        Assert.Equal(7, result.Code.Length);
        Assert.NotEmpty(result.ManagementKey);
        Assert.Equal($"http://localhost:5000/{result.Code}", result.ShortUrl);
    }

    [Fact]
    public async Task CreateAsync_WhenCodeAlreadyExists_ThrowsDuplicateException()
    {
        var repository = new InMemoryLinkRepository();
        var service = CreateService(repository);
        await service.CreateAsync("https://example.com", "ABCD2FG", null);

        var exception = await Assert.ThrowsAsync<LinkDuplicateCodeException>(() => service.CreateAsync("https://example.org", "ABCD2FG", null));

        Assert.Contains("Já existe um link com este código", exception.Message);
    }

    [Fact]
    public async Task GetStatsAsync_WhenManagementKeyIsInvalid_ThrowsUnauthorizedAccessException()
    {
        var repository = new InMemoryLinkRepository();
        var service = CreateService(repository);
        var created = await service.CreateAsync("https://example.com", "ABCD2FG", null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetStatsAsync(created.Code, "chave-errada"));
    }

    private static LinkService CreateService(InMemoryLinkRepository repository)
    {
        return new LinkService(repository, new LinkAccessCache(), new LinkOptions { BaseUrl = "http://localhost:5000" });
    }

    private sealed class InMemoryLinkRepository : ILinkRepository
    {
        private readonly List<Link> _links = new();

        public Task AddAsync(Link link)
        {
            link.Id = _links.Count + 1;
            _links.Add(link);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string code)
        {
            return Task.FromResult(_links.Any(item => item.Code == code));
        }

        public Task<Link?> GetByCodeAsync(string code)
        {
            return Task.FromResult(_links.FirstOrDefault(item => item.Code == code));
        }

        public Task IncrementAccessCountAsync(int id)
        {
            var link = _links.First(item => item.Id == id);
            link.AccessCount += 1;
            return Task.CompletedTask;
        }
    }
}
