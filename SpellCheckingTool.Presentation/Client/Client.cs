using SpellCheckingTool.Application.PersistenceService;
using SpellCheckingTool.Infrastructure.FilePersistence;
using SpellCheckingTool.Domain.WordTree;
using SpellCheckingTool.Infrastructure.UserPersistence;
using SpellCheckingTool.Application.UserService;
using SpellCheckingTool.Domain.Alphabet;

namespace SpellCheckingTool.Presentation.Client;
    public class Client
    {
        public static void StartClient(int port)
        {
            string backendUrl = $"http://localhost:{port}";

            var store = new FileUserStore(Path.Combine(AppContext.BaseDirectory, "data"), new LatinAlphabet());
            var userService = new UserService(store, store);

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

            var wordTree = LoadWordTree();
            var processManager = StartProcessManager();
            StartSpellChecker(wordTree, processManager);
        }

        private static WordTree LoadWordTree()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"../../../.."));
            string path = Path.Combine(projectRoot, @"TestProject/Resources/wordFile.json");

            if (!File.Exists(path))
                throw new FileNotFoundException($"word file not found: {path}");

            var filePath = new FilePath(path);
            var tree = new WordTree();
            return new FilePersistenceService(tree).Load(filePath);
        }
        private static ProcessManager StartProcessManager()
        {
            var processManager = new ProcessManager();
            processManager.Start();
            return processManager;
        }

        private static void StartSpellChecker(WordTree tree, ProcessManager processManager)
        {
            var consoleSpellChecker = new ConsoleSpellChecker(tree, processManager, new SuggestionWindow(tree)
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
            });
            consoleSpellChecker.Run();
        }
    }