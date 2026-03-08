using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Dictionary;

public interface IDefaultDictionaryProvider
{
    WordTree LoadDefaultDictionary();
}