using SpellCheckingTool.Application.Dictionary;
using SpellCheckingTool.Application.PersistenceService;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Infrastructure.Dictionary;

public class DictionaryLoader : IDictionaryLoader
{
    private readonly IPersistenceService persistenceService;

    public DictionaryLoader(IPersistenceService persistenceService)
    {
        this.persistenceService = persistenceService;
    }

    public WordTree LoadDefaultDictionary()
    {
        // Keep your existing path logic for now (functionality step later)
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"../../../.."));
        string path = Path.Combine(projectRoot, @"TestProject/Resources/wordFile.json");

        if (!File.Exists(path))
            throw new FileNotFoundException($"word file not found: {path}");

        var filePath = new FilePath(path);
        return persistenceService.Load(filePath);
    }
}