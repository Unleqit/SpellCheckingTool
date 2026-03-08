using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Users;

public interface IUserCustomDictionaryRepository
{
    void AddWord(Guid userId, string word);
    IReadOnlyCollection<Word> GetWords(Guid userId);
}