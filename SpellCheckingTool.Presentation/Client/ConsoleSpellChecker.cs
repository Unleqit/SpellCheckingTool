using SpellCheckingTool.Application.Spellcheck;

namespace SpellCheckingTool.Presentation.Client;

public class ConsoleSpellChecker
{
    private readonly ISpellcheckService _spellcheckService;
    private readonly ProcessManager _processManager;
    private readonly ISuggestionDisplay _suggestionDisplay;

    private const string WelcomeMessage = "Type text and press space to check words.";

    public ConsoleSpellChecker(
        ISpellcheckService spellcheckService,
        ProcessManager processManager,
        ISuggestionDisplay suggestionWindow)
    {
        _spellcheckService = spellcheckService;
        _processManager = processManager;
        _suggestionDisplay = suggestionWindow;
    }

    public void Run()
    {
        Console.WriteLine(WelcomeMessage);
        string input = "";

        while (true)
        {
            int cursorLeft = Console.CursorLeft;
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            char c = keyInfo.KeyChar;

            switch (keyInfo.Key)
            {
                default:
                    //ignore control chars like Ctrl, Alt, Super, ...
                    if (char.IsControl(c))
                        break;

                    input += c;
                    Console.Write(c);
                    _suggestionDisplay.ShowSuggestionsForString(ref input);
                    break;

                case ConsoleKey.Backspace:
                case (ConsoleKey)127:
                    if (input == "")
                        break;

                    input = input.Substring(0, input.Length - 1);
                    _suggestionDisplay.ShowSuggestionsForString(ref input);
                    break;

                case ConsoleKey.Enter:
                    if (_suggestionDisplay.isCurrentlyVisible())
                        _suggestionDisplay.autoCompleteCurrentlySelectedSuggestion(ref input);
                    else
                    {
                        Console.WriteLine();
                        _processManager.SendInput(input);
                        input = "";
                    }
                    break;

                case ConsoleKey.UpArrow:
                    _suggestionDisplay.selectPreviousSuggestion();
                    break;

                case ConsoleKey.DownArrow:
                    _suggestionDisplay.selectNextSuggestion();
                    break;

                case ConsoleKey.Escape:
                    _suggestionDisplay.hideSuggestions();
                    break;
            }
        }
    }
}