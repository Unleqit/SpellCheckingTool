using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordStats;
using SpellCheckingTool.Domain.WordTree;

namespace TestProject.Fakes;

public class SpyWordDataRepository :
    IUserWordStatsRepository,
    IUserCustomDictionaryRepository
{
    private readonly IAlphabet _alphabet;

    private readonly Dictionary<Guid, Dictionary<string, WordStatistic>> _stats =
        new();

    private readonly Dictionary<Guid, HashSet<Word>> _customWords =
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

    private string? Normalize(string word)
    {
        var normalized = word.Trim().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private Word? NormalizeAndValidateWord(string word)
    {
        var normalized = Normalize(word);
        if (normalized is null)
            return null;

        return new Word(_alphabet, normalized);
    }

    public void IncrementWord(Guid userId, string word)
    {
        IncrementWordCallCount++;
        LastIncrementUserId = userId;
        LastIncrementWord = word;

        var normalized = Normalize(word);
        if (normalized is null)
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

        var wordObj = NormalizeAndValidateWord(word);
        if (wordObj is null)
            return;

        if (!_customWords.TryGetValue(userId, out var words))
        {
            words = new HashSet<Word>();
            _customWords[userId] = words;
        }

        words.Add(wordObj);
    }

    public bool RemoveWord(Guid userId, string word)
    {
        RemoveCustomWordCallCount++;
        LastRemovedCustomUserId = userId;
        LastRemovedCustomWord = word;

        if (!_customWords.TryGetValue(userId, out var words))
            return false;

        var wordObj = NormalizeAndValidateWord(word);
        if (wordObj is null)
            return false;

        return words.Remove(wordObj);
    }

    public IReadOnlyCollection<Word> GetWords(Guid userId)
    {
        if (!_customWords.TryGetValue(userId, out var words))
            return Array.Empty<Word>();

        return words
            .OrderBy(w => w.ToString(), StringComparer.OrdinalIgnoreCase)
            .ToList()
            .AsReadOnly();
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