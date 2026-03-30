using SpellCheckingTool.Application.UserWordStats;
using SpellCheckingTool.Domain.WordStats;

namespace SpellCheckingTool.Application.UserStatsResponse;

public class UserStatsResponse
{
    public Guid UserId { get; set; }
    public List<WordStatistic> Stats { get; set; } = new();
}

public class UserStatsResponseDto
{
    public Guid UserId { get; set; }
    public List<WordStatisticDto> Stats { get; set; } = new();
}