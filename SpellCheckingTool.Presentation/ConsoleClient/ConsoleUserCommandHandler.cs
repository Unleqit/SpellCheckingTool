using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Domain.WordTree;
using System.Diagnostics;

namespace SpellCheckingTool.Presentation.ConsoleClient;

public class ConsoleUserCommandHandler
{
    private readonly UserSpellcheckContext _context;
    private readonly ISuggestionDisplay _suggestionDisplay;
    private readonly ClientAuthService _authService;
    private readonly IUserSpellcheckContextFactory _spellcheckContextFactory;
    private readonly IFileOpener _fileOpener;

    private const int MaxDisplayedStats = 5;

    private delegate void CommandHandler(string command, ref string input);

    public ConsoleUserCommandHandler(
        UserSpellcheckContext context,
        ISuggestionDisplay suggestionDisplay,
        ClientAuthService authService,
        IUserSpellcheckContextFactory spellcheckContextFactory,
        IFileOpener fileOpener)
    {
        _context = context;
        _suggestionDisplay = suggestionDisplay;
        _authService = authService;
        _spellcheckContextFactory = spellcheckContextFactory;
        _fileOpener = fileOpener;
    }

    public bool TryHandleCommand(string input)
    {
        string trimmed = input.Trim();

        if (!trimmed.StartsWith("/"))
            return false;

        Console.WriteLine();

        var commands = new Dictionary<string, CommandHandler>(StringComparer.OrdinalIgnoreCase)
        {
            { "/addword", HandleAddWordCommand },
            { "/delword", HandleDeleteWordCommand },
            { "/words", HandleWordsCommandWrapper },
            { "/stats", HandleStatsCommandWrapper },
            { "/settings", HandleSettingsCommandWrapper }
        };

        var commandName = trimmed.Split(' ', 2)[0];

        if (commands.TryGetValue(commandName, out var handler))
        {
            handler(trimmed, ref input);
            return true;
        }

        Console.WriteLine($"Unknown command: {trimmed}");
        ResetInput(ref input);
        return true;
    }

    private void HandleWordsCommandWrapper(string command, ref string input)
    {
        HandleWordsCommand(ref input);
    }

    private void HandleStatsCommandWrapper(string command, ref string input)
    {
        HandleStatsCommand(ref input);
    }

    private void HandleSettingsCommandWrapper(string command, ref string input)
    {
        HandleSettingsCommand(ref input);
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
            word = new Word(_context.SpellcheckService.Alphabet, normalized);
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
            Console.WriteLine($"Word '{normalized}' already exists in the active tree.");
        }

        bool existsInActiveTree = _context.SpellcheckService.IsCorrect(word);

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

        var userSettings = _context.Settings;
        int maxDisplayedStats = userSettings.MaxDisplayedStats;

        var topStats = stats
            .OrderByDescending(s => s.UsageCount)
            .ThenBy(s => s.Word, StringComparer.OrdinalIgnoreCase)
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

        ResetInput(ref input);
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

    private void ResetInput(ref string input)
    {
        input = "";
        _suggestionDisplay.HideSuggestions();
    }

    private void HandleSettingsCommand(ref string input)
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

        ResetInput(ref input);
    }
}