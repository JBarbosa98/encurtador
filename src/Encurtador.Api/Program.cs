
using Encurtador.Api.Application;
using Encurtador.Api.Controllers;
using Encurtador.Api.Infrastructure.Data;
using Encurtador.Api.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using AppLinkOptions = Encurtador.Api.Application.LinkOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppLinkOptions>(builder.Configuration.GetSection(AppLinkOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=encurtador.db";
builder.Services.AddDbContext<EncurtadorDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<ILinkRepository, SqliteLinkRepository>();
builder.Services.AddSingleton<LinkAccessCache>();

var linkOptions = builder.Configuration.GetSection(AppLinkOptions.SectionName).Get<AppLinkOptions>() ?? new AppLinkOptions();
builder.Services.AddScoped<LinkService>(_ => new LinkService(
    _.GetRequiredService<ILinkRepository>(),
    _.GetRequiredService<LinkAccessCache>(),
    linkOptions));

builder.Services.AddProblemDetails();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EncurtadorDbContext>();
    dbContext.Database.Migrate();
}

app.MapLinkEndpoints();

app.Run();

public partial class Program
{
}
