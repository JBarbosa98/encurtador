using Encurtador.Api.Domain;
using Encurtador.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Encurtador.Api.Infrastructure.Repositories;

public class SqliteLinkRepository : ILinkRepository
{
    private readonly EncurtadorDbContext _dbContext;

    public SqliteLinkRepository(EncurtadorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Link link)
    {
        _dbContext.Links.Add(link);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string code)
    {
        return await _dbContext.Links.AnyAsync(link => link.Code == code);
    }

    public async Task<Link?> GetByCodeAsync(string code)
    {
        return await _dbContext.Links.FirstOrDefaultAsync(link => link.Code == code);
    }

    public async Task IncrementAccessCountAsync(int id)
    {
        await _dbContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE Links SET AccessCount = AccessCount + 1 WHERE Id = {id}");
    }
}
