using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Domain.WordTree;
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

    public ConsoleUserCommandHandler(
        UserSpellcheckContext context,
        ISuggestionDisplay suggestionDisplay,
        ClientUserService clientUserService,
        IFileOpener fileOpener,
        CancellationTokenSource cts,
        IWordService wordService)
    {
        _context = context;
        _suggestionDisplay = suggestionDisplay;
        _clientUserService = clientUserService;
        _cts = cts;
        _wordService = wordService;
        _settingsService = new SettingsService(context, fileOpener);

        _commandHandlers = new Dictionary<string, CommandHandlerAsync>(StringComparer.OrdinalIgnoreCase)
        {
            { "/addword", HandleAddWordCommand },
            { "/delword", HandleDeleteWordCommand },
            { "/stats", (cmd, input) => HandleStatsCommand(input) },
            { "/settings", (cmd, input) => HandleSettingsCommand(input) }, 
            { "/shutdown", HandleShutdownCommand },
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

    private Task<string> HandleShutdownCommand(string command, string input)
    {
        Console.WriteLine("Shutting down...");
        _cts.Cancel();
        return Task.FromResult(ResetInput());
    }
}