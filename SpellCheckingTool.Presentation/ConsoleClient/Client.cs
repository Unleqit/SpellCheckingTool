using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Suggestion;

namespace SpellCheckingTool.Presentation.ConsoleClient;

public class Client
{
    public static void StartClient(int port, IUserSpellcheckContextFactory spellcheckContextFactory)
    {
        string backendUrl = $"http://localhost:{port}";

        var authService = new ClientAuthService(backendUrl);

        Console.Write("Do you want to log in? (y/n): ");
        string input = Console.ReadLine()?.Trim().ToLower() ?? "";

        UserSpellcheckContext context;

        if (input.Contains('y'))
        {
            var session = authService.RunAuthenticationFlow();
            Console.WriteLine();

            context = session != null && session.IsAuthenticated
                ? spellcheckContextFactory.CreateForUser(session.UserId, session.Username)
                : spellcheckContextFactory.CreateAnonymous();
        }
        else
        {
            Console.WriteLine("Skipping authentication." + Environment.NewLine);
            context = spellcheckContextFactory.CreateAnonymous();
        }

        if (context.IsAuthenticated)
        {
            Console.WriteLine($"Loaded spellcheck context for '{context.Username}'.");
        }
        else
        {
            Console.WriteLine("Loaded default spellcheck context.");
        }

        var processManager = StartProcessManager();
        StartSpellChecker(context, authService, processManager, spellcheckContextFactory);
    }

    private static ShellProcessManager StartProcessManager()
    {
        var processManager = new ShellProcessManager();
        processManager.Start();
        return processManager;
    }

    private static void StartSpellChecker(UserSpellcheckContext context, ClientAuthService authService, ShellProcessManager processManager, IUserSpellcheckContextFactory spellcheckContextFactory)
    {
        var settings = context.Settings;

        var suggestionUseCase = new SuggestionUseCase(context.SpellcheckService)
        {
            MaxSuggestions = settings.MaxSuggestions,
            MaxDistance = settings.MaxDistance
        };

        var suggestionWindow = new SuggestionWindow()
        {
            SuggestionBackColor = settings.SuggestionBackColor,
            SuggestionForeColor = settings.SuggestionForeColor,
            CurrentlySelectedSuggestionBackColor = settings.SelectedSuggestionBackColor,
            CurrentlySelectedSuggestionForeColor = settings.SelectedSuggestionForeColor,
            ValidWordBackColor = settings.ValidWordBackColor,
            ValidWordForeColor = settings.ValidWordForeColor,
            InvalidWordBackColor = settings.InvalidWordBackColor,
            InvalidWordForeColor = settings.InvalidWordForeColor,
            HorizontalPaddingSz = settings.HorizontalPadding,
            CurrentlySelectedLine = 0
        };

        var consoleSpellChecker = new ConsoleSpellChecker(
            context,
            suggestionUseCase,
            processManager,
            suggestionWindow,
            authService,
            spellcheckContextFactory);

        consoleSpellChecker.Run();
    }
}