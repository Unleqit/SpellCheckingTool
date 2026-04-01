using SpellCheckingTool.Application.Dictionary;
using SpellCheckingTool.Application.Persistence;
using SpellCheckingTool.Domain.Users;
using SpellCheckingTool.Domain.WordTree;
using SpellCheckingTool.Infrastructure.Dictionary.Exceptions;
using System.Resources;

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
        string path = Path.Combine(projectRoot, "SpellCheckingTool.Presentation", "Resources", "wordFile.json");

        if (!File.Exists(path))
            throw new DefaultDictionaryNotFoundException(path);

        var filePath = new FilePath(path);
        return loader.Load(filePath);
    }
}