using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Dictionary;

public interface IDictionaryLoader
{
    WordTree LoadDefaultDictionary();
}