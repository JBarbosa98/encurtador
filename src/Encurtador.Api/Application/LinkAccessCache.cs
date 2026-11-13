using System.Collections.Concurrent;

namespace Encurtador.Api.Application;

public sealed class LinkAccessCache
{
    private readonly ConcurrentDictionary<string, int> _counts = new(StringComparer.OrdinalIgnoreCase);

    public void RecordAccess(string code)
    {
        _counts.AddOrUpdate(code, 1, (_, current) => current + 1);
    }

    public IReadOnlyCollection<KeyValuePair<string, int>> Top(int take = 10)
    {
        return _counts
            .OrderByDescending(pair => pair.Value)
            .Take(take)
            .ToList();
    }
}
