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

    private static ProcessManager StartProcessManager()
    {
        var processManager = new ProcessManager();
        processManager.Start();
        return processManager;
    }

    private static void StartSpellChecker(
    UserSpellcheckContext context,
    ClientAuthService authService,
    ProcessManager processManager,
    IUserSpellcheckContextFactory spellcheckContextFactory)
    {
        var suggestionUseCase = new SuggestionUseCase(context.SpellcheckService)
        {
            MaxSuggestions = 5,
            MaxDistance = 3
        };

        var suggestionWindow = new SuggestionWindow()
        {
            SuggestionBackColor = ConsoleColor.Red,
            SuggestionForeColor = ConsoleColor.White,
            CurrentlySelectedSuggestionBackColor = ConsoleColor.Yellow,
            CurrentlySelectedSuggestionForeColor = ConsoleColor.Cyan,
            ValidWordBackColor = Console.BackgroundColor,
            ValidWordForeColor = ConsoleColor.Green,
            InvalidWordBackColor = Console.BackgroundColor,
            InvalidWordForeColor = ConsoleColor.Red,
            CurrentlySelectedLine = 0,
            HorizontalPaddingSz = 3
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