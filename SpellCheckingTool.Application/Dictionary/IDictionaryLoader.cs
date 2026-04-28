using SpellCheckingTool.Application.Persistence;
using SpellCheckingTool.Domain;

namespace SpellCheckingTool.Application.Dictionary;

public interface IDictionaryLoader
{
    IWordStorage Load(FilePath filepath);
}