namespace SpellCheckingTool.Infrastructure.FilePersistence.Mappers;

using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.Exceptions;
using SpellCheckingTool.Domain.WordTree;

public class CustomDictionaryStorageMapper
{
    private readonly IAlphabet _alphabet;

    public CustomDictionaryStorageMapper(IAlphabet alphabet)
    {
        _alphabet = alphabet;
    }

    public Dictionary<Guid, HashSet<Word>> ToDomain(Dictionary<Guid, List<string>> storage)
    {
        var result = new Dictionary<Guid, HashSet<Word>>();

        foreach (var userEntry in storage)
        {
            var words = new HashSet<Word>();

            foreach (var raw in userEntry.Value)
            {
                var normalized = raw?.Trim().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(normalized))
                    continue;

                try
                {
                    words.Add(new Word(_alphabet, normalized));
                }
                catch (SpellCheckingToolException ex)
                {
                    Console.WriteLine($"Skipping invalid custom dictionary word '{normalized}': {ex.Message}");
                }
            }

            result[userEntry.Key] = words;
        }

        return result;
    }

    public Dictionary<Guid, List<string>> ToStorage(Dictionary<Guid, HashSet<Word>> domain)
    {
        return domain.ToDictionary(
            userEntry => userEntry.Key,
            userEntry => userEntry.Value
                .Select(w => w.ToString())
                .OrderBy(w => w, StringComparer.OrdinalIgnoreCase)
                .ToList());
    }
}