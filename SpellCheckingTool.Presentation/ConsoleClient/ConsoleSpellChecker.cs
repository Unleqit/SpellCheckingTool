using Newtonsoft.Json.Linq;
using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Presentation.ConsoleClient;

public class ConsoleSpellChecker
{
    private readonly ShellProcessManager _processManager;
    private readonly UserSpellcheckContext _context;
    private readonly ISuggestionDisplay _suggestionDisplay;
    private readonly ClientAuthService _authService;
    private readonly IUserSpellcheckContextFactory _spellcheckContextFactory;
    private readonly ConsoleUserCommandHandler _commandHandler;
    private readonly IFileOpener _fileOpener;
    private readonly CancellationToken _token;
    private readonly UserSettings _settings;

    private SuggestionUseCase _suggestionUseCase;

    private const string WelcomeMessage =
        "Type text and press Enter. Commands: /addword <word>, /delword <word>, /words, /stats";

    public ConsoleSpellChecker(
        UserSpellcheckContext context,
        SuggestionUseCase suggestionUseCase,
        ShellProcessManager processManager,
        ISuggestionDisplay suggestionWindow,
        ClientAuthService authService,
        IUserSpellcheckContextFactory spellcheckContextFactory,
        CancellationToken token,
        UserSettings settings,
        IFileOpener fileOpener)
    {
        _context = context;
        _suggestionUseCase = suggestionUseCase;
        _processManager = processManager;
        _suggestionDisplay = suggestionWindow;
        _authService = authService;
        _spellcheckContextFactory = spellcheckContextFactory;
        _fileOpener = fileOpener;
        _token = token;
        _settings = settings;

        _commandHandler = new ConsoleUserCommandHandler(
            _context,
            _suggestionDisplay,
            _authService,
            _spellcheckContextFactory,
            fileOpener);
    }

    private void UpdateSuggestions(string input)
    {
        if (input.StartsWith("/"))
            return;

        var viewModel = _suggestionUseCase.Execute(input);
        _suggestionDisplay.ShowSuggestions(viewModel);
    }
    
    public void Run()
    {
        Console.WriteLine(WelcomeMessage);
        string input = "";

        string shellPrompt = _processManager.GetCurrentConsolePrompt();
        Console.Write(shellPrompt);
        _suggestionDisplay.Initialize(shellPrompt.Length);

        while (!_token.IsCancellationRequested)
        {
            if (!Console.KeyAvailable)
            {
                Thread.Sleep(50);
                continue;
            }

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

                    TrackLastCompletedWordOnSpace(input);
                    UpdateSuggestions(input);
                    break;

                case ConsoleKey.Enter:
                    if (_commandHandler.TryHandleCommand(input))
                    {
                        RefreshSpellcheckState();
                        Console.Write(_processManager.GetCurrentConsolePrompt());
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
                        TrackFinalWordOnEnter(input.Trim());

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

        _suggestionUseCase = CreateSuggestionUseCase(_context.SpellcheckService);
    }

    private static SuggestionUseCase CreateSuggestionUseCase(ISpellcheckService spellcheckService)
    {
        return new SuggestionUseCase(spellcheckService)
        {
            MaxSuggestions = 5,
            MaxDistance = 3
        };
    }

    private void TrackLastCompletedWordOnSpace(string input)
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
        TrackSingleWord(lastCompletedToken);
    }

    private void TrackFinalWordOnEnter(string input)
    {
        if (!_context.IsAuthenticated || _context.UserId == null)
            return;

        if (string.IsNullOrWhiteSpace(input))
            return;

        var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 0)
            return;

        string lastToken = tokens[^1].Trim().ToLowerInvariant();
        TrackSingleWord(lastToken);
    }

    private void TrackSingleWord(string token)
    {
        if (!_context.IsAuthenticated || _context.UserId == null)
            return;

        if (string.IsNullOrWhiteSpace(token))
            return;

        try
        {
            var word = new Word(_context.SpellcheckService.Alphabet, token);

            if (_context.SpellcheckService.IsCorrect(word))
            {
                _authService.TrackWordUsage(_context.UserId.Value, token);
            }
        }
        catch
        {
            // ignore invalid token
        }
    }
}
