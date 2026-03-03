using SpellCheckingTool.Application.Persistence;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Dictionary;

public class DictionaryLoader : IDictionaryLoader
{
    private readonly IPersistenceService persistenceService;

    public DictionaryLoader(IPersistenceService persistenceService)
    {
        this.persistenceService = persistenceService;
    }

    public WordTree Load(FilePath filepath)
    {
        return persistenceService.Load(filepath);
    }
}