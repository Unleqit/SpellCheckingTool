using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Users;

public interface IUserCustomDictionaryRepository
{
    bool AddWord(Guid userId, string word);
    bool RemoveWord(Guid userId, string word);
    IReadOnlyCollection<Word> GetWords(Guid userId);
}