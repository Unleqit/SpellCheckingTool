namespace SpellCheckingTool.Client
{
    public class ConsoleSpellChecker
    {
        private readonly WordTree _tree;
        private readonly ProcessManager _processManager;
        private readonly ISuggestionDisplay _suggestionDisplay;

        private const string WelcomeMessage = "Type text and press space to check words.";

        public ConsoleSpellChecker(WordTree tree, ProcessManager processManager, ISuggestionDisplay suggestionWindow)
        {
            _tree = tree;
            _processManager = processManager;
            _suggestionDisplay = suggestionWindow;
        }

        public void Run()
        {
            Console.WriteLine(WelcomeMessage);

            string input = "";
            bool enterCommand = false;

            while (true)
            {
                int cursorLeft = Console.CursorLeft;
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                char c = keyInfo.KeyChar;

                switch (keyInfo.Key)
                {
                    default:
                        input += c;
                        Console.Write(c);
                        _suggestionDisplay.ShowSuggestionsForString(ref input);
                        enterCommand = false;
                        break;

                    case ConsoleKey.Enter:
                        if (enterCommand)
                        {
                            Console.WriteLine();
                            _processManager.SendInput(input);
                            _suggestionDisplay.ResetCursorTracking();
                            input = "";
                            enterCommand = false;
                        }
                        else
                        {
                            _suggestionDisplay.autoCompleteCurrentlySelectedSuggestion(ref input);
                            enterCommand = true;
                        }
                        break;

                    case ConsoleKey.UpArrow:
                        _suggestionDisplay.selectPreviousSuggestion();
                        enterCommand = false;
                        break;

                    case ConsoleKey.DownArrow:
                        _suggestionDisplay.selectNextSuggestion();
                        enterCommand = false;
                        break;

                    case ConsoleKey.Escape:
                        _suggestionDisplay.hideSuggestions();
                        enterCommand = true;
                        break;
                }
            }
        }
    }
}

