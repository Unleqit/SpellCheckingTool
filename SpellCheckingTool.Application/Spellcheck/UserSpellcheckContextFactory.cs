using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.WordTree;
using SpellCheckingTool.Application.Dictionary;
using SpellCheckingTool.Domain.Alphabet;
public class UserSpellcheckContextFactory : IUserSpellcheckContextFactory
{
    private readonly IDefaultDictionaryProvider _defaultDictionaryProvider;
    private readonly UserService _userService;
    private readonly IAlphabet _inputAlphabet;

    public UserSpellcheckContextFactory(IDefaultDictionaryProvider defaultDictionaryProvider, UserService userService, IAlphabet inputAlphabet)
    {
        _defaultDictionaryProvider = defaultDictionaryProvider;
        _userService = userService;
        _inputAlphabet = inputAlphabet;
    }

    public UserSpellcheckContext CreateAnonymous()
    {
        var tree = _defaultDictionaryProvider.LoadDefaultDictionary();
        var spellcheckService = BuildSpellcheckService(tree, _inputAlphabet);

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
                    Console.WriteLine($"Error occurred while adding word {word} to the tree");
                }
            }
        }

        var spellcheckService = BuildSpellcheckService(tree, _inputAlphabet);

        return new UserSpellcheckContext(
            userId: userId,
            username: username,
            tree: tree,
            spellcheckService: spellcheckService);
    }

    private static SpellcheckService BuildSpellcheckService(WordTree tree, IAlphabet inputAlphabet)
    {
        ISuggestionService suggestionService =
            new SuggestionService(tree, new LevenshteinDistanceAlgorithm());

        return new SpellcheckService(tree, suggestionService, inputAlphabet);
    }
}