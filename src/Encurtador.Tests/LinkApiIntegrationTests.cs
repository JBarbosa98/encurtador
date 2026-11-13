using System.Net;
using System.Net.Http.Json;
using Encurtador.Api;
using Encurtador.Api.Controllers;
using Encurtador.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Encurtador.Tests;

public class LinkApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public LinkApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<EncurtadorDbContext>));
                if (dbContextDescriptor is not null)
                {
                    services.Remove(dbContextDescriptor);
                }

                var connection = new SqliteConnection("Data Source=:memory:");
                connection.Open();

                services.AddSingleton<DbContextOptions<EncurtadorDbContext>>(_ =>
                    new DbContextOptionsBuilder<EncurtadorDbContext>()
                        .UseSqlite(connection)
                        .Options);

                services.AddSingleton(connection);

                using var scope = services.BuildServiceProvider().CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<EncurtadorDbContext>();
                dbContext.Database.EnsureCreated();
            });
        });
    }

    [Fact]
    public async Task PostApiLinks_ReturnsCreatedResponseAndRedirectWorks()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var createRequest = new
        {
            targetUrl = "https://example.com",
            code = "ABCD2FG"
        };

        var createResponse = await client.PostAsJsonAsync("/api/links", createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<CreateLinkResponse>();
        Assert.NotNull(created);
        Assert.Equal("ABCD2FG", created.Code);

        var redirectResponse = await client.GetAsync($"/{created.Code}");
        Assert.Equal(HttpStatusCode.Redirect, redirectResponse.StatusCode);

        var statsResponse = await client.GetAsync($"/api/links/{created.Code}/stats");
        Assert.Equal(HttpStatusCode.Unauthorized, statsResponse.StatusCode);
    }
}
