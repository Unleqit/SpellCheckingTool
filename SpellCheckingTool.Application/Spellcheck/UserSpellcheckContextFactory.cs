using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.WordTree;
using SpellCheckingTool.Application.Dictionary;
public sealed class UserSpellcheckContextFactory : IUserSpellcheckContextFactory
{
    private readonly IDefaultDictionaryProvider _defaultDictionaryProvider;
    private readonly UserService _userService;

    public UserSpellcheckContextFactory(
    IDefaultDictionaryProvider defaultDictionaryProvider,
    UserService userService)
    {
        _defaultDictionaryProvider = defaultDictionaryProvider;
        _userService = userService;
    }

    public UserSpellcheckContext CreateAnonymous()
    {
        var tree = _defaultDictionaryProvider.LoadDefaultDictionary();
        var spellcheckService = BuildSpellcheckService(tree);

        return new UserSpellcheckContext(
            userId: null,
            username: null,
            tree: tree,
            spellcheckService: spellcheckService);
    }

    public UserSpellcheckContext CreateForUser(Guid userId, string username)
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
                catch
                {

                }
            }
        }

        var spellcheckService = BuildSpellcheckService(tree);

        return new UserSpellcheckContext(
            userId: userId,
            username: username,
            tree: tree,
            spellcheckService: spellcheckService);
    }

    private static ISpellcheckService BuildSpellcheckService(WordTree tree)
    {
        ISuggestionService suggestionService =
            new SuggestionService(tree, new LevenshteinDistanceAlgorithm());

        return new SpellcheckService(tree, suggestionService);
    }
}