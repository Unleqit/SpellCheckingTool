using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Application.Suggestion.SuggestionService;
using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordTree;
using SpellCheckingTool.Application.Executables;

namespace SpellCheckingTool.Application.Spellcheck;

public class UserSpellcheckContextFactory : IUserSpellcheckContextFactory
{
    private readonly UserWordTreeBuilder _treeBuilder;
    private readonly UserService _userService;
    private readonly IAlphabet _inputAlphabet;
    private readonly IUserSettingsRepository _settingsRepository;
    private readonly IExecutableParser _executableParser;

    public UserSpellcheckContextFactory(
        UserWordTreeBuilder treeBuilder,
        UserService userService,
        IUserSettingsRepository settingsRepository,
        IAlphabet inputAlphabet,
        IExecutableParser executableParser)
    {
        _treeBuilder = treeBuilder;
        _userService = userService;
        _settingsRepository = settingsRepository;
        _inputAlphabet = inputAlphabet;
        _executableParser = executableParser;
    }

    public UserSpellcheckContext CreateAnonymous()
    {
        var tree = _treeBuilder.BuildAnonymousTree();
        var executableTree = _executableParser.GetAllShellExecutables();
        var spellcheckService = BuildSpellcheckService(tree, _inputAlphabet, _userService, null);
        var executableService = BuildSpellcheckService(executableTree, _inputAlphabet, _userService, null);
        var settings = _settingsRepository.GetDefaultSettings();

        return new UserSpellcheckContext(
            userId: null,
            username: null,
            tree: tree,
            spellcheckService: spellcheckService,
            executableSpellcheckService: executableService,
            settings: settings,
            settingsRepository: _settingsRepository);
    }

    public UserSpellcheckContext CreateForUser(Guid userId, string username)
    {
        var tree = _treeBuilder.BuildUserTree(userId);
        var executableTree = _executableParser.GetAllShellExecutables();
        var spellcheckService = BuildSpellcheckService(tree, _inputAlphabet, _userService, null);
        var executableService = BuildSpellcheckService(executableTree, _inputAlphabet, _userService, null);
        var settings = _settingsRepository.GetSettings(username);
      
        return new UserSpellcheckContext(
            userId: userId,
            username: username,
            tree: tree,
            spellcheckService: spellcheckService,
            executableSpellcheckService: executableService,
            settings: settings,
            settingsRepository: _settingsRepository);
    }

    private static SpellcheckService BuildSpellcheckService(
        WordTree tree,
        IAlphabet inputAlphabet,
        UserService userService,
        Guid? guid)
    {
        ISuggestionService suggestionService =
            new StatisticSuggestionService(
                tree,
                new LevenshteinDistanceAlgorithm(),
                userService,
                guid ?? Guid.Empty);

        return new SpellcheckService(tree, suggestionService, inputAlphabet);
    }
}