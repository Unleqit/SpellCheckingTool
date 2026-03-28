using SpellCheckingTool.Infrastructure.UserPersistence;

namespace SpellCheckingTool.Infrastructure.FilePersistence.Mappers;

using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordStats;
using SpellCheckingTool.Domain.WordTree;

public class WordStatisticStorageMapper
{
    private readonly IAlphabet _alphabet;

    public WordStatisticStorageMapper(IAlphabet alphabet)
    {
        _alphabet = alphabet;
    }

    public Dictionary<Guid, Dictionary<string, WordInfo>> ToDomain(
        Dictionary<Guid, Dictionary<string, WordStatisticStorage>> storage)
    {
        var result = new Dictionary<Guid, Dictionary<string, WordInfo>>();

        foreach (var userEntry in storage)
        {
            var inner = new Dictionary<string, WordInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var wordEntry in userEntry.Value)
            {
                var storageStat = wordEntry.Value;
                var wordObj = new Word(_alphabet, storageStat.Word);
                var stat = new WordStatistic(wordObj, storageStat.UsageCount, storageStat.LastUsedAt);

                inner[wordEntry.Key] = new WordInfo(wordEntry.Key, stat);
            }

            result[userEntry.Key] = inner;
        }

        return result;
    }

    public Dictionary<Guid, Dictionary<string, WordStatisticStorage>> ToStorage(
        Dictionary<Guid, Dictionary<string, WordInfo>> domain)
    {
        return domain.ToDictionary(
            userEntry => userEntry.Key,
            userEntry => userEntry.Value.ToDictionary(
                wordEntry => wordEntry.Key,
                wordEntry => new WordStatisticStorage
                {
                    Word = wordEntry.Value.Statistic.Word.ToString(),
                    UsageCount = wordEntry.Value.Statistic.UsageCount,
                    LastUsedAt = wordEntry.Value.Statistic.LastUsedAt
                }));
    }
}