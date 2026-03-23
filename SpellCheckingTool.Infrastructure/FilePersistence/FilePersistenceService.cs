using Newtonsoft.Json;
using SpellCheckingTool.Application.Persistence;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordTree;
using SpellCheckingTool.Infrastructure.FilePersistence.Exceptions;

namespace SpellCheckingTool.Infrastructure.FilePersistence;

public class FilePersistenceService : IPersistenceService
{
    /// <summary>
    /// Serializes a WordTree into a .json file
    /// </summary>
    public bool Save(WordTree tree, FilePath filepath)
    {
        string path = filepath.Path;

        if (tree == null)
            throw new TreeNotSpecifiedException();

        if (!path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            throw new UnsupportedFileFormatException(path);

        var walker = new LeftToRightWordTreeTraversal(tree);

        string[] words = new string[tree.WordCount];
        int i = 0;

        walker.WalkTree(word => words[i++] = word.ToString());

        WordTreeDto wordTreeDto = new WordTreeDto()
        {
            alphabet = tree.Alphabet.GetChars(),
            words = words
        };

        string json = JsonConvert.SerializeObject(wordTreeDto, Formatting.Indented);
        File.WriteAllText(path, json);

        return true;
    }

    /// <summary>
    /// Loads a serialized .json file and parses it into a WordTree
    /// </summary>
    public WordTree Load(FilePath filepath)
    {
        string path = filepath.Path;

        if (!path.EndsWith(".json"))
            throw new UnsupportedFileFormatException(path);

        string json = File.ReadAllText(path);

        try
        {
            WordTreeDto dto = JsonConvert.DeserializeObject<WordTreeDto>(json);
            IAlphabet alphabet = new CustomAlphabet(dto.alphabet);
            WordTree tree = new WordTree(alphabet);
            Word[] parsedWords = Word.ParseWords(alphabet, dto.words);
            tree.Add(parsedWords);
            return tree;
        }
        catch
        {
            throw new WordTreeDeserializationException(path);
        }
    }
}