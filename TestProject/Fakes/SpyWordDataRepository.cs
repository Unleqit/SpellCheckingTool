using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordStats;
using SpellCheckingTool.Domain.WordTree;

namespace TestProject.Fakes;

public sealed class SpyWordDataRepository :
    IUserWordStatsRepository,
    IUserCustomDictionaryRepository
{
    private readonly IAlphabet _alphabet;

    private readonly Dictionary<Guid, Dictionary<string, WordStatistic>> _stats =
        new();

    private readonly Dictionary<Guid, HashSet<string>> _customWords =
        new();

    public int IncrementWordCallCount { get; private set; }
    public int AddCustomWordCallCount { get; private set; }
    public int RemoveCustomWordCallCount { get; private set; }

    public Guid? LastIncrementUserId { get; private set; }
    public string? LastIncrementWord { get; private set; }

    public Guid? LastAddedCustomUserId { get; private set; }
    public string? LastAddedCustomWord { get; private set; }

    public Guid? LastRemovedCustomUserId { get; private set; }
    public string? LastRemovedCustomWord { get; private set; }

    public SpyWordDataRepository(IAlphabet alphabet)
    {
        _alphabet = alphabet;
    }

    public void IncrementWord(Guid userId, string word)
    {
        IncrementWordCallCount++;
        LastIncrementUserId = userId;
        LastIncrementWord = word;

        var normalized = word.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
            return;

        if (!_stats.TryGetValue(userId, out var userStats))
        {
            userStats = new Dictionary<string, WordStatistic>(StringComparer.OrdinalIgnoreCase);
            _stats[userId] = userStats;
        }

        if (!userStats.TryGetValue(normalized, out var stat))
        {
            stat = new WordStatistic(new Word(_alphabet, normalized));
            userStats[normalized] = stat;
        }

        stat.Increment();
    }

    public IReadOnlyCollection<WordStatistic> GetWordStats(Guid userId)
    {
        if (!_stats.TryGetValue(userId, out var userStats))
            return Array.Empty<WordStatistic>();

        return userStats.Values.ToList().AsReadOnly();
    }

    public void AddWord(Guid userId, string word)
    {
        AddCustomWordCallCount++;
        LastAddedCustomUserId = userId;
        LastAddedCustomWord = word;

        var normalized = word.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
            return;

        _ = new Word(_alphabet, normalized);

        if (!_customWords.TryGetValue(userId, out var words))
        {
            words = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _customWords[userId] = words;
        }

        words.Add(normalized);
    }

    public bool RemoveWord(Guid userId, string word)
    {
        RemoveCustomWordCallCount++;
        LastRemovedCustomUserId = userId;
        LastRemovedCustomWord = word;

        if (!_customWords.TryGetValue(userId, out var words))
            return false;

        var normalized = word.Trim().ToLowerInvariant();
        return words.Remove(normalized);
    }

    public IReadOnlyCollection<Word> GetWords(Guid userId)
    {
        if (!_customWords.TryGetValue(userId, out var words))
            return Array.Empty<Word>();

        return words.Select(w => new Word(_alphabet, w)).ToList().AsReadOnly();
    }

    public void SeedCustomWord(Guid userId, string word)
    {
        AddWord(userId, word);
    }

    public void SeedStat(Guid userId, string word, int count)
    {
        for (int i = 0; i < count; i++)
            IncrementWord(userId, word);
    }
}