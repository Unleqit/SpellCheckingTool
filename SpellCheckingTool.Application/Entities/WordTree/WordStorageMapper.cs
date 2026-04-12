using SpellCheckingTool.Application.WordDto;
using SpellCheckingTool.Application.WordTreeDto;
using SpellCheckingTool.Domain;

public static class WordStorageMapper
{
    public static WordStorageDto ToStorage(IWordStorage domain)
    {
        int i = 0;
        WordDto[] words = new WordDto[domain.GetWordCount()];

        domain.Traverse((word) => words[i++] = WordMapper.ToStorage(word));

        return new WordStorageDto
        {
            alphabet = domain.GetAlphabet().GetChars(),
            words = words
        };
    }
}