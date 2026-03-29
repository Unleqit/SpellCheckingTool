using SpellCheckingTool.Application.UserWordStats;
using SpellCheckingTool.Domain.WordStats;

public static class WordStatisticMapper
{
    public static WordStatisticDto ToStorage(WordStatistic domain)
    {
        return new WordStatisticDto
        {
            Word = WordMapper.ToStorage(domain.Word),
            UsageCount = domain.UsageCount,
            LastUsedAt = domain.LastUsedAt
        };
    }

    public static WordStatistic ToDomain(WordStatisticDto dto)
    {
        return new WordStatistic(WordMapper.ToDomain(dto.Word), dto.UsageCount, dto.LastUsedAt);
    }
}