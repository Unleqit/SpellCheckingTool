using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Domain.WordTree;


namespace SpellCheckingTool.Presentation.ConsoleClient;

public class ConsoleSpellChecker
{
    private readonly UserSpellcheckContext _context;
    private readonly ProcessManager _processManager;
    private readonly ISuggestionDisplay _suggestionDisplay;
    private readonly ClientAuthService _authService;
    private readonly IUserSpellcheckContextFactory _spellcheckContextFactory;
    private ISpellcheckService _spellcheckService;
    private SuggestionUseCase _suggestionUseCase;

    private const int MaxDisplayedStats = 5; //this should be moved later to user parameters/einstellungen

    private const string WelcomeMessage =
    "Type text and press Enter. Commands: /addword <word>, /delword <word>, /words, /stats";

    public ConsoleSpellChecker(
    UserSpellcheckContext context,
    SuggestionUseCase suggestionUseCase,
    ProcessManager processManager,
    ISuggestionDisplay suggestionWindow,
    ClientAuthService authService,
    IUserSpellcheckContextFactory spellcheckContextFactory)
    {
        _context = context;
        _spellcheckService = context.SpellcheckService;
        _suggestionUseCase = suggestionUseCase;
        _processManager = processManager;
        _suggestionDisplay = suggestionWindow;
        _authService = authService;
        _spellcheckContextFactory = spellcheckContextFactory;
    }

    private void UpdateSuggestions(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            _suggestionDisplay.HideSuggestions();
            return;
        }

        if (input.StartsWith("/"))
        {
            _suggestionDisplay.HideSuggestions();
            return;
        }

        var viewModel = _suggestionUseCase.Execute(input);

        _suggestionDisplay.Show(viewModel);
    }

    private void RebuildActiveTreeAfterDictionaryChange()
    {
        if (!_context.IsAuthenticated || _context.UserId == null || string.IsNullOrWhiteSpace(_context.Username))
            return;

        var refreshed = _spellcheckContextFactory.CreateForUser(_context.UserId.Value, _context.Username);

        _context.Tree = refreshed.Tree;
        _context.SpellcheckService = refreshed.SpellcheckService;
        _spellcheckService = refreshed.SpellcheckService;

        _suggestionUseCase = new SuggestionUseCase(_spellcheckService)
        {
            MaxSuggestions = 5,
            MaxDistance = 3
        };

        _suggestionDisplay.HideSuggestions();
    }
    public void Run()
    {
        Console.WriteLine(WelcomeMessage);
        string input = "";

        while (true)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            char c = keyInfo.KeyChar;

            switch (keyInfo.Key)
            {
                default:
                    //ignore control chars like Ctrl, Alt, Super, ...
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

                    _suggestionDisplay.HideSuggestions();
                    input = input.Substring(0, input.Length - 1);

                    if (Console.CursorLeft > 0)
                    {
                        Console.Write("\b \b");
                    }

                    UpdateSuggestions(input);
                    break;

                case ConsoleKey.Spacebar:
                    input += c;
                    Console.Write(c);

                    TrackLastCompletedWordOnSpace(input);
                    UpdateSuggestions(input);
                    break;

                case ConsoleKey.Enter:
                    if (TryHandleCommand(ref input))
                        break;

                    if (_suggestionDisplay.IsCurrentlyVisible())
                    {
                        _suggestionDisplay.AutoCompleteCurrentlySelectedSuggestion(ref input);
                    }
                    else
                    {
                        TrackFinalWordOnEnter(input);
                        Console.WriteLine();
                        _processManager.SendInput(input);
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

    private bool TryHandleCommand(ref string input)
    {
        string trimmed = input.Trim();

        if (!trimmed.StartsWith("/"))
            return false;

        Console.WriteLine();

        if (trimmed.StartsWith("/addword ", StringComparison.OrdinalIgnoreCase))
        {
            HandleAddWordCommand(trimmed, ref input);
            return true;
        }

        if (trimmed.Equals("/addword", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Usage: /addword <word>");
            ResetInput(ref input);
            return true;
        }

        if (trimmed.StartsWith("/delword ", StringComparison.OrdinalIgnoreCase))
        {
            HandleDeleteWordCommand(trimmed, ref input);
            return true;
        }

        if (trimmed.Equals("/delword", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Usage: /delword <word>");
            ResetInput(ref input);
            return true;
        }

        if (trimmed.Equals("/words", StringComparison.OrdinalIgnoreCase))
        {
            HandleWordsCommand(ref input);
            return true;
        }

        if (trimmed.Equals("/stats", StringComparison.OrdinalIgnoreCase))
        {
            HandleStatsCommand(ref input);
            return true;
        }

        Console.WriteLine($"Unknown command: {trimmed}");
        ResetInput(ref input);
        return true;
    }

    private void HandleAddWordCommand(string command, ref string input)
    {
        if (!_context.IsAuthenticated || _context.UserId == null)
        {
            Console.WriteLine("You need to be logged in to save a personal word.");
            ResetInput(ref input);
            return;
        }

        string rawWord = command.Substring("/addword".Length).Trim();

        if (string.IsNullOrWhiteSpace(rawWord))
        {
            Console.WriteLine("Usage: /addword <word>");
            ResetInput(ref input);
            return;
        }

        if (rawWord.Contains(' '))
        {
            Console.WriteLine("Please enter exactly one word.");
            ResetInput(ref input);
            return;
        }

        string normalized = rawWord.ToLowerInvariant();

        Word word;
        try
        {
            word = new Word(_spellcheckService.Alphabet, normalized);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Invalid word '{normalized}': {ex.Message}");
            ResetInput(ref input);
            return;
        }

        bool persisted = _authService.AddWord(_context.UserId.Value, normalized);
        if (!persisted)
        {
            Console.WriteLine($"Word '{normalized}' was not saved.");
            ResetInput(ref input);
            return;
        }

        try
        {
            RebuildActiveTreeAfterDictionaryChange();
        }
        catch
        {
            // already exists in active tree
        }

        bool existsInActiveTree = _spellcheckService.IsCorrect(word);

        if (existsInActiveTree)
            Console.WriteLine($"Saved '{normalized}' to your personal dictionary.");
        else
            Console.WriteLine($"Saved '{normalized}', but verification in the active tree failed.");

        ResetInput(ref input);
    }

    private void HandleDeleteWordCommand(string command, ref string input)
    {
        if (!_context.IsAuthenticated || _context.UserId == null)
        {
            Console.WriteLine("You need to be logged in to delete a personal word.");
            ResetInput(ref input);
            return;
        }

        string rawWord = command.Substring("/delword".Length).Trim();

        if (string.IsNullOrWhiteSpace(rawWord))
        {
            Console.WriteLine("Usage: /delword <word>");
            ResetInput(ref input);
            return;
        }

        if (rawWord.Contains(' '))
        {
            Console.WriteLine("Please enter exactly one word.");
            ResetInput(ref input);
            return;
        }

        string normalized = rawWord.ToLowerInvariant();

        bool deleted = _authService.DeleteWord(_context.UserId.Value, normalized);
        if (!deleted)
        {
            Console.WriteLine($"Word '{normalized}' was not found in your personal dictionary.");
            ResetInput(ref input);
            return;
        }

        // Remove it from the active tree for this session too.
        RebuildActiveTreeAfterDictionaryChange();

        Word word;
        bool stillValid;
        try
        {
            word = new Word(_spellcheckService.Alphabet, normalized);
            stillValid = _spellcheckService.IsCorrect(word);
        }
        catch
        {
            stillValid = false;
        }

        if (stillValid)
            Console.WriteLine($"Deleted '{normalized}' from your personal dictionary, but it is still valid via the default dictionary.");
        else
            Console.WriteLine($"Deleted '{normalized}' from your personal dictionary.");

        ResetInput(ref input);
    }

    private void HandleWordsCommand(ref string input)
    {
        if (!_context.IsAuthenticated || _context.UserId == null)
        {
            Console.WriteLine("You need to be logged in to view your words.");
            ResetInput(ref input);
            return;
        }

        var words = _authService.GetWords(_context.UserId.Value);

        if (words.Count == 0)
        {
            Console.WriteLine("No saved words found.");
            ResetInput(ref input);
            return;
        }

        Console.WriteLine("Personal dictionary:");
        foreach (var item in words.OrderBy(w => w.Word, StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine($"- {item.Word}");
        }

        ResetInput(ref input);
    }

    private void HandleStatsCommand(ref string input)
    {
        if (!_context.IsAuthenticated || _context.UserId == null)
        {
            Console.WriteLine("You need to be logged in to view your stats.");
            ResetInput(ref input);
            return;
        }

        var stats = _authService.GetStats(_context.UserId.Value);

        if (stats.Count == 0)
        {
            Console.WriteLine("No stats found.");
            ResetInput(ref input);
            return;
        }

        var topStats = stats
            .OrderByDescending(s => s.UsageCount)
            .ThenBy(s => s.Word, StringComparer.OrdinalIgnoreCase)
            .Take(MaxDisplayedStats)
            .ToList();

        Console.WriteLine();
        Console.WriteLine("Top used words:");
        Console.WriteLine("────────────────────────────────────────────");
        Console.WriteLine($"{"Word",-15} {"Count",-7} {"Last Used"}");
        Console.WriteLine("────────────────────────────────────────────");

        foreach (var item in topStats)
        {
            Console.WriteLine(
                $"{item.Word,-15} {item.UsageCount,-7} {item.LastUsedAt:yyyy-MM-dd HH:mm:ss}");
        }

        Console.WriteLine("────────────────────────────────────────────");
        Console.WriteLine($"Showing top {topStats.Count} of {stats.Count} tracked words.");
        Console.WriteLine();

        ResetInput(ref input);
    }

    private void TrackLastCompletedWordOnSpace(string input)
    {
        if (!_context.IsAuthenticated || _context.UserId == null)
            return;

        if (string.IsNullOrWhiteSpace(input))
            return;

        // If input ends with a space, the completed word is the token before that space.
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

        var tokens = input
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

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
            var word = new Word(_spellcheckService.Alphabet, token);

            if (_spellcheckService.IsCorrect(word))
            {
                _authService.TrackWordUsage(_context.UserId.Value, token);
            }
        }
        catch
        {
            // ignore invalid tokens
        }
    }

    private void ResetInput(ref string input)
    {
        input = "";
        _suggestionDisplay.HideSuggestions();
    }
}