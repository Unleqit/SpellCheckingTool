using Newtonsoft.Json.Linq;
using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordTree;
using SpellCheckingTool.Presentation.ConsoleClient.ClientServices;

namespace SpellCheckingTool.Presentation.ConsoleClient;

public class ConsoleSpellChecker
{
    private readonly ShellProcessManager _processManager;
    private readonly UserSpellcheckContext _context;
    private readonly ISuggestionDisplay _suggestionDisplay;
    private readonly ClientUserService _clientUserService;
    private readonly IUserSpellcheckContextFactory _spellcheckContextFactory;
    private readonly ConsoleUserCommandHandler _commandHandler;
    private readonly IFileOpener _fileOpener;
    private readonly CancellationToken _token;
    private readonly UserSettings _settings;

    private SuggestionUseCase _suggestionUseCase;

    private const string WelcomeMessage =
        "Type text and press Enter. Commands: /addword <word>, /delword <word>, /words, /stats, /settings, /shutdown";

    public ConsoleSpellChecker(
        UserSpellcheckContext context,
        SuggestionUseCase suggestionUseCase,
        ShellProcessManager processManager,
        ISuggestionDisplay suggestionWindow,
        ClientUserService clientUserService,
        IUserSpellcheckContextFactory spellcheckContextFactory,
        CancellationToken token,
        UserSettings settings,
        IFileOpener fileOpener,
        Action shutdownAction)
    {
        _context = context;
        _suggestionUseCase = suggestionUseCase;
        _processManager = processManager;
        _suggestionDisplay = suggestionWindow;
        _clientUserService = clientUserService;
        _spellcheckContextFactory = spellcheckContextFactory;
        _fileOpener = fileOpener;
        _token = token;
        _settings = settings;

        _commandHandler = new ConsoleUserCommandHandler(
            _context,
            _suggestionDisplay,
            _clientUserService,
            _spellcheckContextFactory,
            fileOpener,
            shutdownAction);
    }

    private void UpdateSuggestions(string input)
    {
        if (string.IsNullOrEmpty(input) || input.StartsWith("/"))
        {
            _suggestionDisplay.HideSuggestions();
            return;
        }

        if (input.EndsWith(' '))
        {
            _suggestionDisplay.HideSuggestions();
            return;
        }

        var viewModel = _suggestionUseCase.Execute(input);
        _suggestionDisplay.ShowSuggestions(viewModel);
    }

    public async Task Run()
    {
        Console.WriteLine(WelcomeMessage);
        string input = "";

        string shellPrompt = _processManager.GetCurrentConsolePrompt();
        Console.Write(shellPrompt);
        _suggestionDisplay.Initialize(shellPrompt.Length);

        while (!_token.IsCancellationRequested)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            char c = keyInfo.KeyChar;

            switch (keyInfo.Key)
            {
                default:
                    if (char.IsControl(c))
                        break;

                    input += c;
                    Console.Write(c);

                    UpdateSuggestions(input);
                    break;

                case ConsoleKey.Backspace:
                case (ConsoleKey)127:
                    if (input.Length == 0)
                        break;

                    if (input.EndsWith(' '))
                        _suggestionDisplay.PreviousWord();

                    _suggestionDisplay.HideSuggestions();
                    input = input[..^1];

                    if (Console.CursorLeft > 0)
                        Console.Write("\b \b");

                    UpdateSuggestions(input);
                    break;

                case ConsoleKey.Spacebar:
                    input += c;
                    Console.Write(c);

                    _suggestionDisplay.HideSuggestions();
                    _suggestionDisplay.NextWord();

                    await TrackLastCompletedWordOnSpace(input);
                    UpdateSuggestions(input);
                    break;

                case ConsoleKey.Enter:
                    var (handled, newInput) = await _commandHandler.TryHandleCommandAsync(input);
                    input = newInput;

                    if (handled)
                    {
                        if (_token.IsCancellationRequested)
                            return;

                        RefreshSpellcheckState();
                        shellPrompt = _processManager.GetCurrentConsolePrompt();
                        Console.Write(shellPrompt);
                        _suggestionDisplay.Initialize(shellPrompt.Length);
                        break;
                    }

                    if (_suggestionDisplay.IsCurrentlyVisible())
                    {
                        Word completion = _suggestionDisplay.CompleteCurrentlySelectedSuggestion();
                        bool useCapitalization = this._settings.EnableCapitalizationInInput && this._settings.AdjustCapitalizationInSuggestions;
                        string completionFormat = useCapitalization ? completion.GetOriginalWordFormat() : completion.ToString();
                        input = input.Substring(0, input.LastIndexOf(' ') + 1) + completionFormat;
                    }
                    else
                    {
                        await TrackFinalWordOnEnter(input.Trim());

                        Console.WriteLine();
                        _processManager.SendInput(input);

                        shellPrompt = _processManager.GetCurrentConsolePrompt();
                        Console.Write(shellPrompt);
                        _suggestionDisplay.Initialize(shellPrompt.Length);

                        input = "";
                    }
                    break;

                case ConsoleKey.UpArrow:
                    _suggestionDisplay.SelectPreviousSuggestion();
                    break;

                case ConsoleKey.DownArrow:
                    _suggestionDisplay.SelectNextSuggestion();
                    break;

                case ConsoleKey.Escape:
                    _suggestionDisplay.HideSuggestions();
                    break;
            }
        }
    }

    private void RefreshSpellcheckState()
    {
        if (!_context.IsAuthenticated || _context.UserId == null)
            return;

        var refreshed = _spellcheckContextFactory.CreateForUser(
            _context.UserId.Value,
            _context.Username);

        _context.Tree = refreshed.Tree;
        _context.SpellcheckService = refreshed.SpellcheckService;
        _context.ExecutableSpellcheckService = refreshed.ExecutableSpellcheckService;

        _suggestionUseCase = CreateSuggestionUseCase(
            _context.SpellcheckService,
            _context.ExecutableSpellcheckService);
    }

    private SuggestionUseCase CreateSuggestionUseCase(
        ISpellcheckService defaultService,
        ISpellcheckService executableService)
    {
        return new SuggestionUseCase(defaultService, executableService)
        {
            MaxSuggestions = _settings.MaxSuggestions,
            MaxDistance = _settings.MaxDistance
        };
    }

    private async Task TrackLastCompletedWordOnSpace(string input)
    {
        if (!_context.IsAuthenticated || _context.UserId == null)
            return;

        if (string.IsNullOrWhiteSpace(input))
            return;

        var tokens = input
            .TrimEnd()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 0)
            return;

        string lastCompletedToken = tokens[^1].Trim().ToLowerInvariant();
        await TrackSingleWord(lastCompletedToken);
    }

    private async Task TrackFinalWordOnEnter(string input)
    {
        if (!_context.IsAuthenticated || _context.UserId == null)
            return;

        if (string.IsNullOrWhiteSpace(input))
            return;

        var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 0)
            return;

        string lastToken = tokens[^1].Trim().ToLowerInvariant();
        await TrackSingleWord(lastToken);
    }

    private async Task TrackSingleWord(string token)
    {
        if (!_context.IsAuthenticated || _context.UserId == null)
            return;

        if (string.IsNullOrWhiteSpace(token))
            return;

        try
        {
            var service = _context.SpellcheckService;

            var word = new Word(service.Alphabet, token);

            if (service.IsCorrect(word))
            {
                await _clientUserService.Words.TrackWordUsage(_context.UserId.Value, token);
            }
        }
        catch
        {
            // ignore invalid token
        }
    }
}
