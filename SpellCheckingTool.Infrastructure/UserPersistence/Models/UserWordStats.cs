using SpellCheckingTool.Domain.WordStats;

namespace SpellCheckingTool.Infrastructure.UserPersistence.Models;

public class UserWordStats
{
    public Dictionary<Guid, Dictionary<string, WordInfo>> Data { get; set; } = new();
}