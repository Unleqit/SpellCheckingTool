using SpellCheckingTool.Application.Entities.WordInfo;
using SpellCheckingTool.Domain.WordStats;

public static class WordInfoMapper
{
    public static WordInfoDto ToStorage(WordInfo domain)
    {
        return new WordInfoDto
        {
            Key = domain.Key,
            Statistic = WordStatisticMapper.ToStorage(domain.Statistic)
        };
    }

    public static WordInfo ToDomain(WordInfoDto dto)
    {
        return new WordInfo(dto.Key, WordStatisticMapper.ToDomain(dto.Statistic));
    }
}