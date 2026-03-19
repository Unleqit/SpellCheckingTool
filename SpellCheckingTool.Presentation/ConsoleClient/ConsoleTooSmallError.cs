
namespace SpellCheckingTool.Presentation.ConsoleClient;
internal class ConsoleSizeChecker
{
    const string consoleWindowTooSmallErrorMessage = "Please resize the console window to fit the suggestion buffer.";
    bool consoleTooSmallErrorMessageDisplayed;
    int errorMessageStartLeft;
    int errorMessageStartTop;

        public void ShowConsoleTooSmallError()
    {
        consoleTooSmallErrorMessageDisplayed = true;

        ConsoleColor originalBackColor = Console.BackgroundColor;
        ConsoleColor originalForeColor = Console.ForegroundColor;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Red;

        this.errorMessageStartLeft = Console.CursorLeft;
        this.errorMessageStartTop = Console.CursorTop;

        Console.Write(consoleWindowTooSmallErrorMessage);

        Console.BackgroundColor = originalBackColor;
        Console.ForegroundColor = originalForeColor;
    }

    public void HideConsoleTooSmallError()
    {
        Console.SetCursorPosition(errorMessageStartLeft, errorMessageStartTop);

        int currentLeft = Console.CursorLeft;
        int currentTop = Console.CursorTop;
        int lengthOfInputWhileInErroneousState = (currentTop - errorMessageStartTop) * Console.WindowWidth + (Console.WindowWidth - errorMessageStartLeft) + currentLeft;
        lengthOfInputWhileInErroneousState = lengthOfInputWhileInErroneousState > consoleWindowTooSmallErrorMessage.Length ? lengthOfInputWhileInErroneousState : consoleWindowTooSmallErrorMessage.Length;

        Console.Write(new string(' ', lengthOfInputWhileInErroneousState));
        Console.SetCursorPosition(errorMessageStartLeft, errorMessageStartTop);
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
