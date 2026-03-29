using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.Exceptions;
using SpellCheckingTool.Domain.WordTree;
using SpellCheckingTool.Infrastructure.UserPersistence.Models;

namespace SpellCheckingTool.Infrastructure.FilePersistence.Mappers;

internal class CustomDictionaryStorageMapper
{
    private readonly IAlphabet _alphabet;

    public CustomDictionaryStorageMapper(IAlphabet alphabet)
    {
        _alphabet = alphabet;
    }

    public CustomDictionary ToDomain(CustomDictionaryDto storage)
    {
        var result = new Dictionary<Guid, HashSet<Word>>();

        foreach (var userEntry in storage.Data)
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
                    Console.WriteLine(
                        $"Skipping invalid custom dictionary word '{normalized}': {ex.Message}");
                }
            }

            result[userEntry.Key] = words;
        }

        return new CustomDictionary
        {
            Data = result
        };
    }

    public CustomDictionaryDto ToStorage(CustomDictionary domain)
    {
        var result = domain.Data.ToDictionary(
            userEntry => userEntry.Key,
            userEntry => userEntry.Value
                .Select(w => w.ToString())
                .OrderBy(w => w, StringComparer.OrdinalIgnoreCase)
                .ToList());

        return new CustomDictionaryDto
        {
            Data = result
        };
    }
}