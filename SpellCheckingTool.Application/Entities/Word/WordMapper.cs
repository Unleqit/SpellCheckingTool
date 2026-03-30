using SpellCheckingTool.Application.WordDto;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordTree;

public static class WordMapper
{
    public static WordDto ToStorage(Word domain)
    {
        return new WordDto
        {
            Word = domain.ToString()
        };
    }

    public static Word ToDomain(WordDto dto)
    {
        var alphabet = new UTF16Alphabet();
        return new Word(alphabet, dto.Word);
    }
}