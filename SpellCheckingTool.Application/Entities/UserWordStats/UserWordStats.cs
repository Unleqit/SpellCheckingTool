using SpellCheckingTool.Application.Entities.WordInfo;
using SpellCheckingTool.Domain.WordStats;

namespace SpellCheckingTool.Application.UserWordStats;

public class UserWordStats
{
    public Dictionary<Guid, Dictionary<string, WordInfo>> Data { get; set; } = new();
}

public class UserWordStatsDto
{
    public Dictionary<Guid, Dictionary<string, WordInfoDto>> Data { get; set; } = new();
}