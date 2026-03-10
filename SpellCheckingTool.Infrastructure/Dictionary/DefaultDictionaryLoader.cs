using SpellCheckingTool.Application.Dictionary;
using SpellCheckingTool.Application.Persistence;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Infrastructure.Dictionary;

public class DefaultDictionaryLoader : IDefaultDictionaryProvider
{
    private readonly IDictionaryLoader loader;

    public DefaultDictionaryLoader(IDictionaryLoader loader)
    {
        this.loader = loader;
    }

    public WordTree LoadDefaultDictionary()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        string path = Path.Combine(projectRoot, "TestProject", "Resources", "wordFile.json");

        if (!File.Exists(path))
            throw new FileNotFoundException($"word file not found: {path}");

        var filePath = new FilePath(path);
        return loader.Load(filePath);
    }
}