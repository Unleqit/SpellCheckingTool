using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Application.Spellcheck;
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

        var context = spellcheckContextFactory.CreateAnonymous();

        Console.WriteLine("Loaded default spellcheck context.");
        Console.WriteLine("Use /login <username> (--register) to log in or register.");
        Console.WriteLine();

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