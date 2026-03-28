using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordStats;
using SpellCheckingTool.Domain.WordTree;

namespace TestProject.Fakes;

public class FakeUserWordStatsRepository : IUserWordStatsRepository
{
    private readonly IAlphabet _alphabet;
    private readonly Dictionary<Guid, Dictionary<string, WordStatistic>> _stats = new();

    public int IncrementWordCallCount { get; private set; }
    public Guid? LastIncrementUserId { get; private set; }
    public string? LastIncrementWord { get; private set; }

    public FakeUserWordStatsRepository(IAlphabet alphabet)
    {
        _alphabet = alphabet;
    }

    private string? Normalize(string word)
    {
        var normalized = word.Trim().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
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
            _stats[userId][normalized] = stat;
        }

        stat.Increment();
    }

    public IReadOnlyCollection<WordStatistic> GetWordStats(Guid userId)
    {
        if (!_stats.TryGetValue(userId, out var userStats))
            return Array.Empty<WordStatistic>();

        return userStats.Values.ToList().AsReadOnly();
    }

    public void SeedStat(Guid userId, string word, int count)
    {
        for (int i = 0; i < count; i++)
            IncrementWord(userId, word);
    }
}