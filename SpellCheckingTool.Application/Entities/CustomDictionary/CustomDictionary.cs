using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.CustomDictionary;

public class CustomDictionary
{
    public Dictionary<Guid, HashSet<Word>> Data { get; set; } = new();
}

public class CustomDictionaryDto
{
    public Dictionary<Guid, List<WordDto.WordDto>> Data { get; set; } = new();
}