using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Domain.WordTree;


namespace SpellCheckingTool.Presentation.ConsoleClient;

public class ConsoleSpellChecker
{
    private readonly UserSpellcheckContext _context;
    private readonly ISpellcheckService _spellcheckService;
    private readonly ProcessManager _processManager;
    private readonly ISuggestionDisplay _suggestionDisplay;
    private readonly SuggestionUseCase _suggestionUseCase;
    private readonly ClientAuthService _authService;

    private const string WelcomeMessage =
        "Type text and press Enter. Use /addword <word> to save a personal word.";

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

    public ConsoleSpellChecker(
        UserSpellcheckContext context,
        SuggestionUseCase suggestionUseCase,
        ProcessManager processManager,
        ISuggestionDisplay suggestionWindow,
        ClientAuthService authService)
    {
        _context = context;
        _spellcheckService = context.SpellcheckService;
        _suggestionUseCase = suggestionUseCase;
        _processManager = processManager;
        _suggestionDisplay = suggestionWindow;
        _authService = authService;
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

                    input = input.Substring(0, input.Length - 1);

                    if (Console.CursorLeft > 0)
                    {
                        Console.Write("\b \b");
                    }

                    UpdateSuggestions(input);
                    break;

                case ConsoleKey.Enter:
                    if (input.StartsWith("/addword ", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine();

                        if (!_context.IsAuthenticated || _context.UserId == null)
                        {
                            Console.WriteLine("You need to be logged in to save a personal word.");
                            input = "";
                            _suggestionDisplay.HideSuggestions();
                            break;
                        }

                        string rawWord = input.Substring("/addword".Length).Trim();

                        if (string.IsNullOrWhiteSpace(rawWord) || rawWord.Contains(' '))
                        {
                            Console.WriteLine("Usage: /addword <word>");
                            input = "";
                            _suggestionDisplay.HideSuggestions();
                            break;
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
                            input = "";
                            _suggestionDisplay.HideSuggestions();
                            break;
                        }

                        bool persisted = _authService.AddWord(_context.UserId.Value, normalized);
                        if (!persisted)
                        {
                            Console.WriteLine($"Word '{normalized}' was not saved.");
                            input = "";
                            _suggestionDisplay.HideSuggestions();
                            break;
                        }

                        try
                        {
                            _context.Tree.Add(word);
                        }
                        catch
                        {
                            // already exists is okay
                        }

                        bool existsNow = _spellcheckService.IsCorrect(word);

                        if (existsNow)
                            Console.WriteLine($"Saved '{normalized}' successfully.");
                        else
                            Console.WriteLine($"Saved '{normalized}', but active-tree verification failed.");

                        input = "";
                        _suggestionDisplay.HideSuggestions();
                        break;
                    }

                    if (_suggestionDisplay.IsCurrentlyVisible())
                        _suggestionDisplay.AutoCompleteCurrentlySelectedSuggestion(ref input);
                    else
                    {
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
            input = "";
            _suggestionDisplay.HideSuggestions();
            return true;
        }

        Console.WriteLine($"Unknown command: {trimmed}");
        input = "";
        _suggestionDisplay.HideSuggestions();
        return true;
    }

    private void HandleAddWordCommand(string command, ref string input)
    {
        if (!_context.IsAuthenticated || _context.UserId == null)
        {
            Console.WriteLine("You need to be logged in to save a personal word.");
            input = "";
            _suggestionDisplay.HideSuggestions();
            return;
        }

        string rawWord = command.Substring("/addword".Length).Trim();

        if (string.IsNullOrWhiteSpace(rawWord))
        {
            Console.WriteLine("Usage: /addword <word>");
            input = "";
            _suggestionDisplay.HideSuggestions();
            return;
        }

        if (rawWord.Contains(' '))
        {
            Console.WriteLine("Please enter exactly one word.");
            input = "";
            _suggestionDisplay.HideSuggestions();
            return;
        }

        string normalized = rawWord.Trim().ToLowerInvariant();

        Word word;
        try
        {
            word = new Word(_spellcheckService.Alphabet, normalized);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Invalid word '{normalized}': {ex.Message}");
            input = "";
            _suggestionDisplay.HideSuggestions();
            return;
        }

        bool persisted = _authService.AddWord(_context.UserId.Value, normalized);
        if (!persisted)
        {
            Console.WriteLine($"Word '{normalized}' was not saved.");
            input = "";
            _suggestionDisplay.HideSuggestions();
            return;
        }

        try
        {
            _context.Tree.Add(word);
        }
        catch
        {
            // If Add throws because it already exists, persistence already succeeded.
            // We continue to the verification step below.
        }

        bool existsInActiveTree = _spellcheckService.IsCorrect(word);

        if (existsInActiveTree)
        {
            Console.WriteLine($"Saved '{normalized}' to your personal dictionary.");
            Console.WriteLine("Verification passed: the word is valid in the active tree now.");
        }
        else
        {
            Console.WriteLine($"Saved '{normalized}', but verification in the active tree failed.");
        }

        input = "";
        _suggestionDisplay.HideSuggestions();
    }
}