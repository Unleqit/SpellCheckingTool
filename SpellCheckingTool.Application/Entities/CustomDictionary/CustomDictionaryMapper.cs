using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.Exceptions;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.CustomDictionary;

public static class CustomDictionaryMapper
{
    public static CustomDictionary ToDomain(CustomDictionaryDto storage)
    {
        var result = new Dictionary<Guid, HashSet<Word>>();
        var alphabet = new UTF16Alphabet();

        foreach (var userEntry in storage.Data)
        {
            var words = new HashSet<Word>();

            foreach (var raw in userEntry.Value)
            {
                try
                {
                    Word domain = WordMapper.ToDomain(raw);
                    words.Add(domain);
                }
                catch (SpellCheckingToolException ex)
                {
                    Console.WriteLine(
                        $"Skipping invalid custom dictionary word '{raw.Word}': {ex.Message}");
                }
            }

            result[userEntry.Key] = words;
        }

        return new CustomDictionary
        {
            Data = result
        };
    }

    public static CustomDictionaryDto ToStorage(CustomDictionary domain)
    {
        var result = domain.Data.ToDictionary(
            userEntry => userEntry.Key,
            userEntry => userEntry.Value
                .Select(w => WordMapper.ToStorage(w))
                .ToList());

        return new CustomDictionaryDto
        {
            Data = result
        };
    }
}