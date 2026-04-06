using SpellCheckingTool.Domain;

namespace SpellCheckingTool.Application.Dictionary;

public interface IDefaultDictionaryProvider
{
    IWordStorage LoadDefaultDictionary();
}