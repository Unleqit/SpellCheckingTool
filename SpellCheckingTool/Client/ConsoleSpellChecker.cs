using System;
using System.Text;
using SpellCheckingTool;

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

            StringBuilder currentWord = new StringBuilder();
            string input = "";

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                char c = keyInfo.KeyChar;

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    HandleEnter(ref input, currentWord);
                    continue;
                }

                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    HandleBackspace(ref input, currentWord);
                    continue;
                }

                input += c;
                HandleCharacter(c, currentWord);
            }
        }

        private void HandleEnter(ref string input, StringBuilder currentWord)
        {
            if (currentWord.Length > 0)
            {
                PrintColoredWord(currentWord, addSpace: false);
                currentWord.Clear();
            }
            Console.WriteLine();
            _processManager.SendInput(input);
            input = "";
        }


        private void HandleCharacter(char c, StringBuilder currentWord)
        {
            if (c == ' ')
            {
                PrintColoredWord(currentWord);
                currentWord.Clear();
                
            }
            else
            {
                currentWord.Append(c);
                Console.Write(c);
            }

        }

        private void PrintColoredWord(StringBuilder currentWord, bool addSpace = true)
        {
            if (currentWord.Length == 0)
            {
                if (addSpace) 
                   Console.Write(' ');
                return;
            }

            string originalWord = currentWord.ToString();
            bool exists = _tree.Contains(originalWord.ToLower());

            // Move the cursor back to the beginning of the current word
            Console.SetCursorPosition(Console.CursorLeft - currentWord.Length, Console.CursorTop);

            if (exists)
                Console.ForegroundColor = ConsoleColor.Green;
            else
                Console.ForegroundColor = ConsoleColor.Red;

            Console.Write(originalWord);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(' ');
        }

        private void HandleBackspace(ref string input, StringBuilder currentWord)
        {
            if (input.Length > 0)
            {

                input = input.Substring(0, input.Length - 1);
                Console.Write("\b \b");

                if (currentWord.Length > 0)
                {
                    // remove last character from the current word
                    currentWord.Remove(currentWord.Length - 1, 1);
                }

                else
                {
                    // if Backspace is pressed after a completed word, restore the previous word
                    int lastSpace = input.LastIndexOf(' ');
                    if (lastSpace >= 0)
                    {
                        string lastWord = input.Substring(lastSpace + 1);
                        currentWord.Clear();
                        currentWord.Append(lastWord);
                    }
                }
            }
        }
    }
}

