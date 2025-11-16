namespace SpellCheckingTool.Client
{
    public class ConsoleSpellChecker
    {
        private readonly WordTree _tree;
        private readonly ProcessManager _processManager;

        private const string WelcomeMessage = "Type text and press space to check words.";

        public ConsoleSpellChecker(WordTree tree, ProcessManager processManager)
        {
            _tree = tree;
            _processManager = processManager;
        }

        public void Run()
        {
            Console.WriteLine(WelcomeMessage);

            string input = "";

            while (true)
            {
                int cursorLeft = Console.CursorLeft;
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                char c = keyInfo.KeyChar;

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter:
                        //the enter key automatically sets the CursorLeft property to 0, hence we need to restore it
                        Console.CursorLeft = cursorLeft;
                        HandleEnter(ref input);
                        break;

                    case ConsoleKey.Backspace:
                        HandleBackspace(ref input);
                        break;

                    case ConsoleKey.Spacebar:
                        string lastWord = GetLastWord(input);
                        CheckAndColorWord(lastWord + c);
                        input += c;
                        break;

                    default:
                        input += c;
                        break;
                }
            }
        }

        private void HandleEnter(ref string input)
        {
            string lastWord = GetLastWord(input);

            if (lastWord.Length > 0)
                CheckAndColorWord(lastWord);

            Console.WriteLine();
            _processManager.SendInput(input);
            input = "";
        }

        private void CheckAndColorWord(string currentWord)
        {
            string lookupWord = currentWord.Trim().ToLower();
            bool exists = _tree.Contains(lookupWord);

            ConsoleColor color = exists ? ConsoleColor.Green : ConsoleColor.Red;

            PrintColoredWord(currentWord, color);
        }

        private void PrintColoredWord(string currentWord, ConsoleColor color)
        {
            // Move the cursor back to the beginning of the current word
            Console.SetCursorPosition(Console.CursorLeft - currentWord.Length, Console.CursorTop);

            Console.ForegroundColor = color;
            Console.Write(currentWord);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private void HandleBackspace(ref string input)
        {
            if (input.Length > 0)
            {
                input = input.Substring(0, input.Length - 1);
                Console.Write(" \b");

                string lastWord = GetLastWord(input);
                PrintColoredWord(lastWord, ConsoleColor.White);
            }
        }

        private string GetLastWord(string input)
        {
            int lastSpace = input.LastIndexOf(' ');
            return lastSpace >= 0 ? input.Substring(lastSpace + 1) : input;
        }

    }
}

