using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.PersistenceService;

public interface IPersistenceService
{
    bool Save(WordTree tree, FilePath filepath);
    WordTree Load(FilePath filepath);
}