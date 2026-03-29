namespace SpellCheckingTool.Infrastructure.UserPersistence.Models;

public class UserWordStatsDto
{
    public Dictionary<Guid, Dictionary<string, WordStatisticStorage>> Data { get; set; } = new();
}