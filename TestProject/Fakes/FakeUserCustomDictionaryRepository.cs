using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordTree;

namespace TestProject.Fakes;

public class FakeUserCustomDictionaryRepository : IUserCustomDictionaryRepository
{
    private readonly IAlphabet _alphabet;
    private readonly Dictionary<Guid, HashSet<Word>> _customWords = new();

    public int AddCustomWordCallCount { get; private set; }
    public int RemoveCustomWordCallCount { get; private set; }

    public Guid? LastAddedCustomUserId { get; private set; }
    public string? LastAddedCustomWord { get; private set; }

    public Guid? LastRemovedCustomUserId { get; private set; }
    public string? LastRemovedCustomWord { get; private set; }

    public FakeUserCustomDictionaryRepository(IAlphabet alphabet)
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

    public bool AddWord(Guid userId, string word)
    {
        AddCustomWordCallCount++;
        LastAddedCustomUserId = userId;
        LastAddedCustomWord = word;

        var wordObj = NormalizeAndValidateWord(word);
        if (wordObj is null)
            return false;

        if (!_customWords.TryGetValue(userId, out var words))
        {
            words = new HashSet<Word>();
            _customWords[userId] = words;
        }

        return words.Add(wordObj);
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
        _ = AddWord(userId, word);
    }
}