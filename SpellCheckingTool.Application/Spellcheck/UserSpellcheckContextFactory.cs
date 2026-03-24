using SpellCheckingTool.Application.Dictionary;
using SpellCheckingTool.Application.Dictionary;
using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.Exceptions;
using SpellCheckingTool.Domain.WordTree;
public class UserSpellcheckContextFactory : IUserSpellcheckContextFactory
{
    private readonly IDefaultDictionaryProvider _defaultDictionaryProvider;
    private readonly UserService _userService;
    private readonly IAlphabet _inputAlphabet;
    private readonly IUserSettingsRepository _settingsRepository;

    public UserSpellcheckContextFactory(
    IDefaultDictionaryProvider defaultDictionaryProvider,
    UserService userService,
    IUserSettingsRepository settingsRepository,
    IAlphabet inputAlphabet)
    {
        _defaultDictionaryProvider = defaultDictionaryProvider;
        _userService = userService;
        _settingsRepository = settingsRepository;
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
            spellcheckService: spellcheckService,
            settings: UserSettings.Default);
    }

    public UserSpellcheckContext CreateForUser(Guid userId, string username)
    {
        var tree = _defaultDictionaryProvider.LoadDefaultDictionary();

        var settings = _settingsRepository.GetSettings(username);

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

        var spellcheckService = BuildSpellcheckService(tree, _inputAlphabet);

        return new UserSpellcheckContext(
            userId: userId,
            username: username,
            tree: tree,
            spellcheckService: spellcheckService,
            settings: settings);
    }

    private static SpellcheckService BuildSpellcheckService(WordTree tree, IAlphabet inputAlphabet)
    {
        ISuggestionService suggestionService =
            new SuggestionService(tree, new LevenshteinDistanceAlgorithm());

        return new SpellcheckService(tree, suggestionService, inputAlphabet);
    }
}