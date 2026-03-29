namespace SpellCheckingTool.Application.UserWordStats;

public class WordStatisticDto
{
    public WordDto.WordDto Word { get; set; } = null!;
    public int UsageCount { get; set; }
    public DateTime LastUsedAt { get; set; }
}