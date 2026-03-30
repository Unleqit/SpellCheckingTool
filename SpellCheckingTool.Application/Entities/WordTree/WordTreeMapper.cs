using SpellCheckingTool.Application.WordDto;
using SpellCheckingTool.Application.WordTreeDto;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordTree;

public static class WordTreeMapper
{
    public static WordTreeDto ToStorage(WordTree domain)
    {
        var walkService = new LeftToRightWordTreeTraversal(domain);
        int i = 0;
        WordDto[] words = new WordDto[domain.WordCount];

        walkService.WalkTree((word) => words[i++] = WordMapper.ToStorage(word));

        return new WordTreeDto
        {
            alphabet = domain.alphabet.GetChars(),
            words = words
        };
    }

    public static WordTree ToDomain(WordTreeDto dto)
    {
        var alphabet = new CustomAlphabet(dto.alphabet);
        Word[] words = dto.words.Select((wordDto) => WordMapper.ToDomain(wordDto)).ToArray();
        var tree = new WordTree(alphabet);
        tree.Add(words);
        return tree;
    }
}