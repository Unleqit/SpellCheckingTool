using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SpellCheckingTool.Client
{
    public class Client
    {
        public static void StartClient(int port)
        {
            var wordTree = LoadWordTree();
            var processManager = StartProcessManager();
            StartSpellChecker(wordTree, processManager);
        }
        private static WordTree LoadWordTree()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\.."));
            string path = Path.Combine(projectRoot, @"TestProject\Resources\wordFile.wdb");

            if (!File.Exists(path))
                throw new FileNotFoundException($"Wörterbuchdatei nicht gefunden: {path}");

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
}