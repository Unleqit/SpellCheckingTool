using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Presentation.ConsoleClient.ClientServices;

namespace SpellCheckingTool.Presentation.ConsoleClient;

public class Client
{
    public static async Task StartClient(int port, IUserSpellcheckContextFactory spellcheckContextFactory, IFileOpener fileOpener, CancellationTokenSource token)
    {
        string backendUrl = $"http://localhost:{port}";
        using var httpClient = new HttpClient();

        var backendClient = new BackendClient(httpClient, backendUrl);
        var authService = new ClientAuthService(backendClient);
        var wordService = new ClientWordService(backendClient);
        var statsService = new ClientStatsService(backendClient);

        var clientUserService = new ClientUserService(authService, wordService, statsService);

        Console.Write("Do you want to log in? (y/n): ");
        string input = Console.ReadLine()?.Trim().ToLower() ?? "";

        UserSpellcheckContext context;

        if (input.Contains('y'))
        {
            var session = await clientUserService.Auth.RunAuthenticationFlow();
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
        await StartSpellChecker(context, clientUserService, processManager, spellcheckContextFactory, fileOpener, token);
    }

    private static ShellProcessManager StartProcessManager()
    {
        var processManager = new ShellProcessManager();
        processManager.Start();
        return processManager;
    }

    private static async Task StartSpellChecker(UserSpellcheckContext context, ClientUserService clientUserService, ShellProcessManager processManager, IUserSpellcheckContextFactory spellcheckContextFactory, IFileOpener fileOpener, CancellationTokenSource token)
    {
        var suggestionDisplay = new SuggestionDisplay(context.Settings);
        var consoleSpellChecker = new ConsoleSpellChecker(
            context,
            processManager,
            suggestionDisplay,
            clientUserService,
            spellcheckContextFactory,
            token,
            fileOpener);
        await consoleSpellChecker.Run();
    }
}