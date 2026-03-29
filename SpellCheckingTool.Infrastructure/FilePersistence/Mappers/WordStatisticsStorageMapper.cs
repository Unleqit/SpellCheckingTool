using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordStats;
using SpellCheckingTool.Domain.WordTree;
using SpellCheckingTool.Infrastructure.UserPersistence;
using SpellCheckingTool.Infrastructure.UserPersistence.Models;

namespace SpellCheckingTool.Infrastructure.FilePersistence.Mappers;

internal class WordStatisticStorageMapper
{
    private readonly IAlphabet _alphabet;

    public WordStatisticStorageMapper(IAlphabet alphabet)
    {
        _alphabet = alphabet;
    }

    public UserWordStats ToDomain(UserWordStatsDto storage)
    {
        var result = new Dictionary<Guid, Dictionary<string, WordInfo>>();

        foreach (var userEntry in storage.Data)
        {
            var inner = new Dictionary<string, WordInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var wordEntry in userEntry.Value)
            {
                var storageStat = wordEntry.Value;
                var wordObj = new Word(_alphabet, storageStat.Word);
                var stat = new WordStatistic(
                    wordObj,
                    storageStat.UsageCount,
                    storageStat.LastUsedAt);

                inner[wordEntry.Key] = new WordInfo(wordEntry.Key, stat);
            }

            result[userEntry.Key] = inner;
        }

        return new UserWordStats
        {
            Data = result
        };
    }

    public UserWordStatsDto ToStorage(UserWordStats domain)
    {
        var result = domain.Data.ToDictionary(
            userEntry => userEntry.Key,
            userEntry => userEntry.Value.ToDictionary(
                wordEntry => wordEntry.Key,
                wordEntry => new WordStatisticStorage
                {
                    Word = wordEntry.Value.Statistic.Word.ToString(),
                    UsageCount = wordEntry.Value.Statistic.UsageCount,
                    LastUsedAt = wordEntry.Value.Statistic.LastUsedAt
                }));

        return new UserWordStatsDto
        {
            Data = result
        };
    }
}