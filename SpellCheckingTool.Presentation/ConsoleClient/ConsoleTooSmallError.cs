
namespace SpellCheckingTool.Presentation.ConsoleClient;
internal class ConsoleSizeChecker
    {
        const string consoleWindowTooSmallErrorMessage = "Please resize the console window to fit the suggestion buffer.";
        bool consoleTooSmallErrorMessageDisplayed;

        public void ShowConsoleTooSmallError()
        {
            consoleTooSmallErrorMessageDisplayed = true;

            ConsoleColor originalBackColor = Console.BackgroundColor;
            ConsoleColor originalForeColor = Console.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.Write(consoleWindowTooSmallErrorMessage);

            Console.BackgroundColor = originalBackColor;
            Console.ForegroundColor = originalForeColor;
        }

        public void HideConsoleTooSmallError()
        {
            Console.Write(new string(' ', consoleWindowTooSmallErrorMessage.Length));
            Console.CursorLeft -= consoleWindowTooSmallErrorMessage.Length;
            consoleTooSmallErrorMessageDisplayed = false;
        }

        public bool IsConsoleWindowTooSmall(int suggestionWindowHeight)
        {
            return (Console.WindowHeight - Console.CursorTop) < (suggestionWindowHeight + 1);
        }

        public bool IsErrorMessageDisplayed()
        {
            return consoleTooSmallErrorMessageDisplayed;
        }
    }
