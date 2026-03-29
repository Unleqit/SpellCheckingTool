using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordStats;

namespace SpellCheckingTool.Application.UserWordStats;

public static class UserWordStatsMapper
{
    public static UserWordStats ToDomain(UserWordStatsDto storage)
    {
        var alphabet = new UTF16Alphabet();
        var result = new Dictionary<Guid, Dictionary<string, WordInfo>>();

        foreach (var userEntry in storage.Data)
        {
            var inner = new Dictionary<string, WordInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var wordEntry in userEntry.Value)
            {
                var dto = wordEntry.Value;
                var tmp = WordInfoMapper.ToDomain(dto);
                inner[wordEntry.Key] = tmp;
            }

            result[userEntry.Key] = inner;
        }

        return new UserWordStats
        {
            Data = result
        };
    }

    public static UserWordStatsDto ToStorage(UserWordStats domain)
    {
        var result = domain.Data.ToDictionary(
            userEntry => userEntry.Key,
            userEntry => userEntry.Value.ToDictionary(
                wordEntry => wordEntry.Key,
                wordEntry => WordInfoMapper.ToStorage(wordEntry.Value)
            ));

        return new UserWordStatsDto
        {
            Data = result
        };
    }
}