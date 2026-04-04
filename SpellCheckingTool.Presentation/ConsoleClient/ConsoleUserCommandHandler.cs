using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Presentation.ConsoleClient.ClientServices;

namespace SpellCheckingTool.Presentation.ConsoleClient;

public class ConsoleUserCommandHandler
{
    private readonly UserSpellcheckContext _context;
    private readonly ISuggestionDisplay _suggestionDisplay;
    private readonly ClientUserService _clientUserService;
    private readonly CancellationTokenSource _cts;
    private readonly IWordService _wordService;
    private readonly SettingsService _settingsService;

    private readonly SemaphoreSlim _commandLock = new(1, 1);

    private delegate Task<string> CommandHandlerAsync(string command, string input);

    private readonly Dictionary<string, CommandHandlerAsync> _commandHandlers;

    private readonly Action _onContextChanged;

    private readonly IUserSpellcheckContextFactory _spellcheckContextFactory;

    public ConsoleUserCommandHandler(
    UserSpellcheckContext context,
    ISuggestionDisplay suggestionDisplay,
    ClientUserService clientUserService,
    IFileOpener fileOpener,
    CancellationTokenSource cts,
    IWordService wordService,
    IUserSpellcheckContextFactory spellcheckContextFactory,
    Action onContextChanged)
    {
        _context = context;
        _suggestionDisplay = suggestionDisplay;
        _clientUserService = clientUserService;
        _cts = cts;
        _wordService = wordService;
        _settingsService = new SettingsService(context, fileOpener);
        _spellcheckContextFactory = spellcheckContextFactory;
        _onContextChanged = onContextChanged;

        _commandHandlers = new Dictionary<string, CommandHandlerAsync>(StringComparer.OrdinalIgnoreCase)
    {
        { "/addword", HandleAddWordCommand },
        { "/delword", HandleDeleteWordCommand },
        { "/stats", (cmd, input) => HandleStatsCommand(input) },
        { "/settings", (cmd, input) => HandleSettingsCommand(input) },
        { "/shutdown", HandleShutdownCommand },
        { "/login", HandleLoginCommand },
        { "/logout", HandleLogoutCommand },
        { "/words", (cmd, input) => HandleWordsCommand() }
    };
    }

    public async Task<(bool handled, string newInput)> TryHandleCommandAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return (false, input);

        string trimmed = input.Trim();
        string commandWord = GetFirstToken(trimmed);

        if (!commandWord.StartsWith("/"))
            return (false, input);

        if (!_commandHandlers.TryGetValue(commandWord, out var handler))
            return (false, input);

        Console.WriteLine();

        await _commandLock.WaitAsync();
        try
        {
            string updatedInput = await handler(trimmed, input);
            return (true, updatedInput);
        }
        finally
        {
            _commandLock.Release();
        }
    }

    private async Task<string> HandleAddWordCommand(string command, string input)
    {
        var (success, message) = await _wordService.AddWordAsync(command);

        Console.WriteLine(message);
        return ResetInput();
    }

    private async Task<string> HandleDeleteWordCommand(string command, string input)
    {
        var (success, message) = await _wordService.DeleteWordAsync(command);

        Console.WriteLine(message);
        return ResetInput();
    }

    private async Task<string> HandleWordsCommand()
    {
        if (!_context.IsAuthenticated || _context.UserId == null)
        {
            Console.WriteLine("You need to be logged in to view your words.");
            return ResetInput();
        }

        var words = await _wordService.GetWordsAsync();

        if (!words.Any())
        {
            Console.WriteLine("No saved words found.");
            return ResetInput();
        }

        Console.WriteLine("Personal dictionary:");
        foreach (var item in words.OrderBy(s => s, StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine($"- {item}");
        }

        return ResetInput();
    }

    private async Task<string> HandleStatsCommand(string input)
    {
        if (!_context.IsAuthenticated || _context.UserId == null)
        {
            Console.WriteLine("You need to be logged in to view your stats.");
            return ResetInput();
        }

        var stats = await _clientUserService.Stats.GetStats(_context.UserId.Value);
        var formatted = StatsFormatter.FormatStats(stats, _context.Settings);
        Console.WriteLine(formatted);
        return ResetInput();
    }

    private string ResetInput()
    {
        _suggestionDisplay.HideSuggestions();
        return "";
    }

    private Task<string> HandleSettingsCommand(string input)
    {
        try
        {
            string path = _settingsService.OpenOrCreateSettingsFile();

            Console.WriteLine($"Settings opened: {path}");
            Console.WriteLine("Note: Restart the application to apply changes.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open settings: {ex.Message}");
        }

        return Task.FromResult(ResetInput());
    }

    private static string GetFirstToken(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        string trimmed = input.Trim();
        int firstSpaceIndex = trimmed.IndexOf(' ');

        return firstSpaceIndex < 0
            ? trimmed
            : trimmed[..firstSpaceIndex];
    }
    private async Task<string> HandleLoginCommand(string command, string input)
    {
        if (_context.IsAuthenticated)
        {
            Console.WriteLine($"Already logged in as '{_context.Username}'. Use /logout first.");
            return ResetInput();
        }

        var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
        {
            Console.WriteLine("Usage: /login <username> [--register]");
            return ResetInput();
        }

        string username = parts[1];
        bool isRegister = parts.Skip(2).Any(p =>
            string.Equals(p, "--register", StringComparison.OrdinalIgnoreCase));

        var session = await _clientUserService.Auth.RunAuthenticationFlow(username, isRegister);

        if (session == null || !session.IsAuthenticated)
        {
            Console.WriteLine("Login failed.");
            return ResetInput();
        }

        var userContext = _spellcheckContextFactory.CreateForUser(session.UserId, session.Username);

        _context.ReplaceWith(userContext);
        _onContextChanged();

        Console.WriteLine($"Loaded spellcheck context for '{_context.Username}'.");
        return ResetInput();
    }

    private Task<string> HandleLogoutCommand(string command, string input)
    {
        if (!_context.IsAuthenticated)
        {
            Console.WriteLine("You are already in default mode.");
            return Task.FromResult(ResetInput());
        }

        var anonymous = _spellcheckContextFactory.CreateAnonymous();
        _context.ReplaceWith(anonymous);
        _onContextChanged();


        _suggestionDisplay.HideSuggestions();
        Console.WriteLine("Logged out. Loaded default spellcheck context.");
        return Task.FromResult(ResetInput());
    }

    private Task<string> HandleShutdownCommand(string command, string input)
    {
        Console.WriteLine("Shutting down...");
        _cts.Cancel();
        return Task.FromResult(ResetInput());
    }
}