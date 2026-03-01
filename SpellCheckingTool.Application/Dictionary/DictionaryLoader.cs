using SpellCheckingTool.Application.PersistenceService;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Dictionary;

public interface IDictionaryLoader
{
    WordTree Load(FilePath filepath);
}