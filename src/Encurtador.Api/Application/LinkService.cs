using System.Security.Cryptography;
using System.Text;
using Encurtador.Api.Domain;
using Encurtador.Api.Infrastructure.Repositories;

namespace Encurtador.Api.Application;

public sealed record CreateLinkResult(string Code, string ShortUrl, string ManagementKey);

public sealed record LinkStatsResult(string Code, string TargetUrl, DateTime CreatedAtUtc, DateTime? ExpiresAtUtc, int AccessCount);

public sealed class LinkService
{
    private const string AllowedCodeCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int CodeLength = 7;

    private readonly ILinkRepository _repository;
    private readonly LinkAccessCache _cache;
    private readonly LinkOptions _options;

    public LinkService(ILinkRepository repository, LinkAccessCache cache, LinkOptions options)
    {
        _repository = repository;
        _cache = cache;
        _options = options;
    }

    public async Task<CreateLinkResult> CreateAsync(string targetUrl, string? code, DateTime? expiresAtUtc)
    {
        var normalizedTargetUrl = targetUrl.Trim();

        if (!LinkUrlValidator.IsValid(normalizedTargetUrl))
        {
            throw new LinkValidationException("A URL de destino é inválida.");
        }

        var normalizedCode = string.IsNullOrWhiteSpace(code) ? LinkCodeGenerator.Generate() : code.Trim();

        if (normalizedCode.Length != CodeLength || normalizedCode.Any(character => !AllowedCodeCharacters.Contains(character)))
        {
            throw new LinkValidationException("O código deve ter 7 caracteres com o alfabeto válido.");
        }

        if (await _repository.ExistsAsync(normalizedCode))
        {
            throw new LinkDuplicateCodeException("Já existe um link com este código.");
        }

        var managementKey = Guid.NewGuid().ToString("N");
        var link = new Link
        {
            Code = normalizedCode,
            TargetUrl = normalizedTargetUrl,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = expiresAtUtc?.ToUniversalTime(),
            AccessCount = 0,
            ManagementKeyHash = HashManagementKey(managementKey)
        };

        await _repository.AddAsync(link);

        return new CreateLinkResult(normalizedCode, $"{_options.BaseUrl.TrimEnd('/')}/{normalizedCode}", managementKey);
    }

    public async Task<(bool Found, bool Expired, string? TargetUrl)> RedirectAsync(string code)
    {
        var link = await _repository.GetByCodeAsync(code);

        if (link is null)
        {
            return (false, false, null);
        }

        if (link.ExpiresAtUtc.HasValue && link.ExpiresAtUtc.Value <= DateTime.UtcNow)
        {
            return (true, true, null);
        }

        await _repository.IncrementAccessCountAsync(link.Id);
        _cache.RecordAccess(code);

        return (true, false, link.TargetUrl);
    }

    public async Task<LinkStatsResult> GetStatsAsync(string code, string managementKey)
    {
        var link = await _repository.GetByCodeAsync(code);

        if (link is null)
        {
            throw new LinkNotFoundException("Link não encontrado.");
        }

        if (link.ExpiresAtUtc.HasValue && link.ExpiresAtUtc.Value <= DateTime.UtcNow)
        {
            throw new LinkExpiredException("O link expirou.");
        }

        if (!VerifyManagementKey(link.ManagementKeyHash, managementKey))
        {
            throw new UnauthorizedAccessException("Chave de gerenciamento inválida.");
        }

        return new LinkStatsResult(link.Code, link.TargetUrl, link.CreatedAtUtc, link.ExpiresAtUtc, link.AccessCount);
    }

    public IReadOnlyCollection<KeyValuePair<string, int>> GetTopLinks(int take = 10)
    {
        return _cache.Top(take);
    }

    private static string HashManagementKey(string managementKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(managementKey));
        return Convert.ToHexString(bytes);
    }

    private static bool VerifyManagementKey(string hash, string input)
    {
        return string.Equals(HashManagementKey(input), hash, StringComparison.OrdinalIgnoreCase);
    }
}
