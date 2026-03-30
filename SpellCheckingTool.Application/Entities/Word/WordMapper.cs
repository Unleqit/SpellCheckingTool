using SpellCheckingTool.Application.WordDto;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordTree;

public static class WordMapper
{
    private static IAlphabet alphabet;

    static WordMapper()
    {
        alphabet = new UTF16Alphabet();
    }

    public static WordDto ToStorage(Word domain)
    {
        return new WordDto
        {
            Word = domain.ToString()
        };
    }

    public static Word ToDomain(WordDto dto)
    {
        return new Word(alphabet, dto.Word);
    }
}