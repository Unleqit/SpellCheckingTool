using SpellCheckingTool.Application.WordTreeDto;
using SpellCheckingTool.Domain;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Infrastructure;

public static class WordTreeMapper
{
    public static WordTree ToDomain(WordStorageDto dto)
    {
        var alphabet = new CustomAlphabet(dto.alphabet);
        Word[] words = dto.words.Select((wordDto) => WordMapper.ToDomain(wordDto)).ToArray();
        var tree = new WordTree(alphabet);
        tree.Add(words);
        return tree;
    }
}