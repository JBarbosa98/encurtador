using Encurtador.Api.Domain;

namespace Encurtador.Api.Infrastructure.Repositories;

public interface ILinkRepository
{
    Task AddAsync(Link link);

    Task<bool> ExistsAsync(string code);

    Task<Link?> GetByCodeAsync(string code);

    Task IncrementAccessCountAsync(int id);
}
