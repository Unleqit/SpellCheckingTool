using SpellCheckingTool.Application.Persistence;
using SpellCheckingTool.Domain;

namespace SpellCheckingTool.Application.Dictionary;

public class DictionaryLoader : IDictionaryLoader
{
    private readonly IPersistenceService persistenceService;

    public DictionaryLoader(IPersistenceService persistenceService)
    {
        this.persistenceService = persistenceService;
    }

    public IWordStorage Load(FilePath filepath)
    {
        return persistenceService.Load(filepath);
    }
}