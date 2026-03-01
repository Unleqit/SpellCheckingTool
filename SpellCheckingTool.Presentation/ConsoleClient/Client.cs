using SpellCheckingTool.Application.Spellcheck;

namespace SpellCheckingTool.Presentation.ConsoleClient;

public class Client
{
    public static void StartClient(int port, ISpellcheckService spellcheckService)
    {
        string backendUrl = $"http://localhost:{port}";

        var authService = new ClientAuthService(backendUrl);

        Console.Write("Do you want to log in? (y/n): ");
        string input = Console.ReadLine()?.Trim().ToLower() ?? "";

        if (input.Contains('y'))
        {
            authService.RunAuthenticationFlow();
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine("Skipping authentication...\n");
        }

        var processManager = StartProcessManager();
        StartSpellChecker(spellcheckService, processManager);
    }

    private static ProcessManager StartProcessManager()
    {
        var processManager = new ProcessManager();
        processManager.Start();
        return processManager;
    }

    private static void StartSpellChecker(ISpellcheckService spellcheckService, ProcessManager processManager)
    {
        var suggestionWindow = new SuggestionWindow(spellcheckService)
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
            HorizontalPaddingSz = 3,
            SuggestionAlgorithmMaxAllowedDistance = 3,
            MaxSuggestionsToBeDisplayed = 7
        };

        var consoleSpellChecker = new ConsoleSpellChecker(
            spellcheckService,
            processManager,
            suggestionWindow);

        consoleSpellChecker.Run();
    }
}