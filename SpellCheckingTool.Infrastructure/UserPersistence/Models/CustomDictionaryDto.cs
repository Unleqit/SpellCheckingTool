namespace SpellCheckingTool.Infrastructure.UserPersistence.Models;

public class CustomDictionaryDto
{
    public Dictionary<Guid, List<string>> Data { get; set; } = new();
}