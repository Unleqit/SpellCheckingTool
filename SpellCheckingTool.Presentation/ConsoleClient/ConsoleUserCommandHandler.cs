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
    private readonly IUserSpellcheckContextFactory _spellcheckContextFactory;
    private readonly IFileOpener _fileOpener;
    private readonly Action _shutdownAction;

    private readonly SemaphoreSlim _commandLock = new(1, 1);

    private delegate Task<string> CommandHandlerAsync(string command, string input);

    private readonly Dictionary<string, CommandHandlerAsync> _commandHandlers;

    public ConsoleUserCommandHandler(
        UserSpellcheckContext context,
        ISuggestionDisplay suggestionDisplay,
        ClientUserService clientUserService,
        IUserSpellcheckContextFactory spellcheckContextFactory,
        IFileOpener fileOpener,
        Action shutdownAction)
    {
        _context = context;
        _suggestionDisplay = suggestionDisplay;
        _clientUserService = clientUserService;
        _spellcheckContextFactory = spellcheckContextFactory;
        _fileOpener = fileOpener;
        _shutdownAction = shutdownAction;

        _commandHandlers = new Dictionary<string, CommandHandlerAsync>(StringComparer.OrdinalIgnoreCase)
        {
            { "/addword", HandleAddWordCommand },
            { "/delword", HandleDeleteWordCommand },
            { "/words", HandleWordsCommandWrapper },
            { "/stats", HandleStatsCommandWrapper },
            { "/settings", HandleSettingsCommandWrapper },
            { "/shutdown", HandleShutdownCommand }
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

    private async Task<string> HandleWordsCommandWrapper(string command, string input)
    {
        return await HandleWordsCommand(input);
    }

    private async Task<string> HandleStatsCommandWrapper(string command, string input)
    {
        return await HandleStatsCommand(input);
    }

    private Task<string> HandleSettingsCommandWrapper(string command, string input)
    {
        return HandleSettingsCommand(input);
    }

    private async Task<string> HandleAddWordCommand(string command, string input)
    {
        if (!_context.IsAuthenticated || _context.UserId == null)
        {
            Console.WriteLine("You need to be logged in to save a personal word.");
            return ResetInput();
        }

        string rawWord = command.Substring("/addword".Length).Trim();

        if (string.IsNullOrWhiteSpace(rawWord))
        {
            Console.WriteLine("Usage: /addword <word>");
            return ResetInput();
        }

        if (rawWord.Contains(' '))
        {
            Console.WriteLine("Please enter exactly one word.");
            return ResetInput();
        }

        string normalized = rawWord.ToLowerInvariant();

        Word word;
        try
        {
            word = new Word(_context.SpellcheckService.Alphabet, normalized);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Invalid word '{normalized}': {ex.Message}");
            return ResetInput();
        }

        bool persisted = await _clientUserService.Words.AddWord(_context.UserId.Value, normalized);
        if (!persisted)
        {
            Console.WriteLine($"Word '{normalized}' was not saved.");
            return ResetInput();
        }

        try
        {
            RebuildActiveTreeAfterDictionaryChange();

            if (!_context.SpellcheckService.IsCorrect(word))
            {
                await _clientUserService.Words.DeleteWord(_context.UserId.Value, normalized);

                try
                {
                    RebuildActiveTreeAfterDictionaryChange();
                }
                catch
                {
                }

                Console.WriteLine($"Invalid word '{normalized}'.");
                return ResetInput();
            }
        }
        catch (Exception ex)
        {
            await _clientUserService.Words.DeleteWord(_context.UserId.Value, normalized);

            try
            {
                RebuildActiveTreeAfterDictionaryChange();
            }
            catch
            {
            }

            Console.WriteLine($"Invalid word '{normalized}': {ex.Message}");
            return ResetInput();
        }

        Console.WriteLine($"Saved '{normalized}' to your personal dictionary.");
        return ResetInput();
    }

    private async Task<string> HandleDeleteWordCommand(string command, string input)
    {
        if (!_context.IsAuthenticated || _context.UserId == null)
        {
            Console.WriteLine("You need to be logged in to delete a personal word.");
            return ResetInput();
        }

        string rawWord = command.Substring("/delword".Length).Trim();

        if (string.IsNullOrWhiteSpace(rawWord))
        {
            Console.WriteLine("Usage: /delword <word>");
            return ResetInput();
        }

        if (rawWord.Contains(' '))
        {
            Console.WriteLine("Please enter exactly one word.");
            return ResetInput();
        }

        string normalized = rawWord.ToLowerInvariant();

        bool deleted = await _clientUserService.Words.DeleteWord(_context.UserId.Value, normalized);
        if (!deleted)
        {
            Console.WriteLine($"Word '{normalized}' was not found in your personal dictionary.");
            return ResetInput();
        }

        RebuildActiveTreeAfterDictionaryChange();

        bool stillValid;
        try
        {
            var word = new Word(_context.SpellcheckService.Alphabet, normalized);
            stillValid = _context.SpellcheckService.IsCorrect(word);
        }
        catch
        {
            stillValid = false;
        }

        if (stillValid)
            Console.WriteLine($"Deleted '{normalized}' from your personal dictionary, but it is still valid via the default dictionary.");
        else
            Console.WriteLine($"Deleted '{normalized}' from your personal dictionary.");

        return ResetInput();
    }

    private async Task<string> HandleWordsCommand(string input)
    {
        if (!_context.IsAuthenticated || _context.UserId == null)
        {
            Console.WriteLine("You need to be logged in to view your words.");
            return ResetInput();
        }

        var words = await _clientUserService.Words.GetWords(_context.UserId.Value);

        if (words.Count() == 0)
        {
            Console.WriteLine("No saved words found.");
            return ResetInput();
        }

        Console.WriteLine("Personal dictionary:");
        foreach (var item in words.OrderBy(w => w.ToString(), StringComparer.OrdinalIgnoreCase))
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

        if (stats.Count == 0)
        {
            Console.WriteLine("No stats found.");
            return ResetInput();
        }

        var userSettings = _context.Settings;
        int maxDisplayedStats = userSettings.MaxDisplayedStats;

        var topStats = stats
            .OrderByDescending(s => s.UsageCount)
            .ThenBy(s => s.Word.ToString(), StringComparer.OrdinalIgnoreCase)
            .Take(maxDisplayedStats)
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

        return ResetInput();
    }

    private void RebuildActiveTreeAfterDictionaryChange()
    {
        if (!_context.IsAuthenticated || _context.UserId == null || string.IsNullOrWhiteSpace(_context.Username))
            return;

        var refreshed = _spellcheckContextFactory.CreateForUser(_context.UserId.Value, _context.Username);

        _context.Tree = refreshed.Tree;
        _context.SpellcheckService = refreshed.SpellcheckService;

        _suggestionDisplay.HideSuggestions();
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
            var username = _context.Username ?? string.Empty;

            var path = _context.SettingsRepository.GetUserSettingsFilePath(
                _context.Username);

            if (!File.Exists(path))
            {
                var settingsToWrite = string.IsNullOrWhiteSpace(username)
                    ? _context.SettingsRepository.GetDefaultSettings()
                    : _context.Settings;

                _context.SettingsRepository.SetSettings(username, settingsToWrite);
            }

            _fileOpener.Open(path);

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
        _shutdownAction?.Invoke();

        return Task.FromResult(ResetInput());
    }
}