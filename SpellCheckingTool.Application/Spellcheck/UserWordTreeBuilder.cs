using SpellCheckingTool.Application.Dictionary;
using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Exceptions;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Spellcheck;

public class UserWordTreeBuilder
{
    private readonly IDefaultDictionaryProvider _defaultDictionaryProvider;
    private readonly UserService _userService;

    public UserWordTreeBuilder(
        IDefaultDictionaryProvider defaultDictionaryProvider,
        UserService userService)
    {
        _defaultDictionaryProvider = defaultDictionaryProvider;
        _userService = userService;
    }

    public WordTree BuildAnonymousTree()
    {
        return _defaultDictionaryProvider.LoadDefaultDictionary();
    }

    public WordTree BuildUserTree(Guid userId)
    {
        var tree = _defaultDictionaryProvider.LoadDefaultDictionary();
        var customWordsResult = _userService.GetCustomWords(userId);

        if (customWordsResult.Success && customWordsResult.Value != null)
        {
            foreach (var word in customWordsResult.Value)
            {
                try
                {
                    tree.Add(word);
                }
                catch (SpellCheckingToolException ex)
                {
                    Console.WriteLine(
                        $"Could not add custom word '{word}' to the user's tree: {ex.Message}");
                }
            }
        }

        return tree;
    }
}