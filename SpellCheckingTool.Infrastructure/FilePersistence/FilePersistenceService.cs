using Newtonsoft.Json;
using SpellCheckingTool.Application.Persistence;
using SpellCheckingTool.Application.WordTreeDto;
using SpellCheckingTool.Domain;
using SpellCheckingTool.Infrastructure.FilePersistence.Exceptions;

namespace SpellCheckingTool.Infrastructure.FilePersistence;

public class FilePersistenceService : IPersistenceService
{
    /// <summary>
    /// Serializes a WordTree into a .json file
    /// </summary>
    public bool Save(IWordStorage tree, FilePath filepath)
    {
        string path = filepath.Path;

        if (tree == null)
            throw new TreeNotSpecifiedException();

        if (!path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            throw new UnsupportedFileFormatException(path);

        var dto = WordStorageMapper.ToStorage(tree);

        string json = JsonConvert.SerializeObject(dto, Formatting.Indented);
        File.WriteAllText(path, json);

        return true;
    }

    /// <summary>
    /// Loads a serialized .json file and parses it into a WordTree
    /// </summary>
    public IWordStorage Load(FilePath filepath)
    {
        string path = filepath.Path;

        if (!path.EndsWith(".json"))
            throw new UnsupportedFileFormatException(path);

        string json = File.ReadAllText(path);

        try
        {
            var dto = JsonConvert.DeserializeObject<WordStorageDto>(json);
            WordTree tree = WordTreeMapper.ToDomain(dto);
            return tree;
        }
        catch
        {
            throw new WordTreeDeserializationException(path);
        }
    }
}
