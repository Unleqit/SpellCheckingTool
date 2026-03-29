using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Infrastructure.UserPersistence.Models;

public class CustomDictionary
{
    public Dictionary<Guid, HashSet<Word>> Data { get; set; } = new();
}