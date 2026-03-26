using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Suggestion;

namespace SpellCheckingTool.Presentation.ConsoleClient;

public class Client
{
    public static void StartClient(int port, IUserSpellcheckContextFactory spellcheckContextFactory, IFileOpener fileOpener, CancellationToken token)
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
        StartSpellChecker(context, authService, processManager, spellcheckContextFactory, fileOpener, token);
    }

    private static ShellProcessManager StartProcessManager()
    {
        var processManager = new ShellProcessManager();
        processManager.Start();
        return processManager;
    }

    private static void StartSpellChecker(UserSpellcheckContext context, ClientAuthService authService, ShellProcessManager processManager, IUserSpellcheckContextFactory spellcheckContextFactory, IFileOpener fileOpener, CancellationToken token)
    {
        var settings = context.Settings;

        var suggestionUseCase = new SuggestionUseCase(context.SpellcheckService)
        {
            MaxSuggestions = settings.MaxSuggestions,
            MaxDistance = settings.MaxDistance
        };
        
        var suggestionDisplay = new SuggestionDisplay(settings);

        var consoleSpellChecker = new ConsoleSpellChecker(
            context,
            suggestionUseCase,
            processManager,
            suggestionDisplay,
            authService,
            spellcheckContextFactory,
            fileOpener,
            token);

        consoleSpellChecker.Run();
    }
}