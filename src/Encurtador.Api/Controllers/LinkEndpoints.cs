using Encurtador.Api.Application;
using Microsoft.AspNetCore.Mvc;

namespace Encurtador.Api.Controllers;

public static class LinkEndpoints
{
    public static void MapLinkEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/links", async (CreateLinkRequest request, LinkService service) =>
        {
            try
            {
                var result = await service.CreateAsync(request.TargetUrl, request.Code, request.ExpiresAtUtc);

                return Results.Created($"/api/links/{result.Code}", new CreateLinkResponse
                {
                    ShortUrl = result.ShortUrl,
                    Code = result.Code,
                    ManagementKey = result.ManagementKey
                });
            }
            catch (LinkValidationException ex)
            {
                return Results.Problem(
                    title: "Dados inválidos",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "https://httpstatuses.com/400");
            }
            catch (LinkDuplicateCodeException ex)
            {
                return Results.Problem(
                    title: "Código duplicado",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status409Conflict,
                    type: "https://httpstatuses.com/409");
            }
        });

        app.MapGet("/{code}", async (string code, LinkService service) =>
        {
            var result = await service.RedirectAsync(code);

            if (!result.Found)
            {
                return Results.NotFound();
            }

            if (result.Expired)
            {
                return Results.Problem(
                    title: "Link expirado",
                    detail: "Este link não está mais disponível.",
                    statusCode: StatusCodes.Status410Gone,
                    type: "https://httpstatuses.com/410");
            }

            return Results.Redirect(result.TargetUrl!);
        });

        app.MapGet("/api/links/{code}/stats", async (string code, HttpContext http, LinkService service) =>
        {
            var managementKey = http.Request.Headers["X-Management-Key"].ToString();

            if (string.IsNullOrWhiteSpace(managementKey))
            {
                return Results.Problem(
                    title: "Chave ausente",
                    detail: "A chave de gerenciamento é obrigatória.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    type: "https://httpstatuses.com/401");
            }

            try
            {
                var stats = await service.GetStatsAsync(code, managementKey);
                return Results.Ok(stats);
            }
            catch (LinkNotFoundException ex)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "Link não encontrado",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch (LinkExpiredException ex)
            {
                return Results.Problem(
                    title: "Link expirado",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status410Gone,
                    type: "https://httpstatuses.com/410");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Problem(
                    title: "Acesso negado",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status401Unauthorized,
                    type: "https://httpstatuses.com/401");
            }
        });
    }
}

public sealed class CreateLinkRequest
{
    public string TargetUrl { get; set; } = string.Empty;

    public string? Code { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }
}

public sealed class CreateLinkResponse
{
    public string ShortUrl { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string ManagementKey { get; set; } = string.Empty;
}
