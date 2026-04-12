using SpellCheckingTool.Domain;

namespace SpellCheckingTool.Application.Persistence;

public interface IPersistenceService
{
    bool Save(IWordStorage tree, FilePath filepath);
    IWordStorage Load(FilePath filepath);
}