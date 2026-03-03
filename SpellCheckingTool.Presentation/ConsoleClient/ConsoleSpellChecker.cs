using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Domain.WordTree;


namespace SpellCheckingTool.Presentation.ConsoleClient;

public class ConsoleSpellChecker
{
    private readonly ISpellcheckService _spellcheckService;
    private readonly ProcessManager _processManager;
    private readonly ISuggestionDisplay _suggestionDisplay;
    private readonly SuggestionUseCase _suggestionUseCase;

    private const string WelcomeMessage = "Type text and press space to check words.";

    private int GetStartIndexOfCurrentWord(string input)
    {
        return input.LastIndexOf(' ') + 1;
    }

    private Word ExtractWord(string input)
    {
        int startIndex = GetStartIndexOfCurrentWord(input);
        string wordString = input.Substring(startIndex);
        return new Word(_spellcheckService.Alphabet, wordString);
    }

    private void UpdateSuggestions(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            _suggestionDisplay.HideSuggestions();
            return;
        }

        int startIndex = GetStartIndexOfCurrentWord(input);
        Word word = ExtractWord(input);

        var viewModel = _suggestionUseCase.Execute(word);

        _suggestionDisplay.Show(viewModel, startIndex);
    }

    public ConsoleSpellChecker(
        ISpellcheckService spellcheckService,
        SuggestionUseCase suggestionUseCase,
        ProcessManager processManager,
        ISuggestionDisplay suggestionWindow)
    {
        _spellcheckService = spellcheckService;
        _suggestionUseCase = suggestionUseCase;
        _processManager = processManager;
        _suggestionDisplay = suggestionWindow;
    }
    public void Run()
    {
        Console.WriteLine(WelcomeMessage);
        string input = "";

        while (true)
        {
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
                    UpdateSuggestions(input);
                    break;

                case ConsoleKey.Backspace:
                case (ConsoleKey)127:
                    if (input == "")
                        break;

                    input = input.Substring(0, input.Length - 1);
                    Console.Write("\b \b");
                    UpdateSuggestions(input);
                    break;

                case ConsoleKey.Enter:
                    if (_suggestionDisplay.IsCurrentlyVisible())
                        _suggestionDisplay.AutoCompleteCurrentlySelectedSuggestion(ref input);
                    else
                    {
                        Console.WriteLine();
                        _processManager.SendInput(input);
                        input = "";
                    }
                    break;

                case ConsoleKey.UpArrow:
                    _suggestionDisplay.SelectPreviousSuggestion();
                    break;

                case ConsoleKey.DownArrow:
                    _suggestionDisplay.SelectNextSuggestion();
                    break;

                case ConsoleKey.Escape:
                    _suggestionDisplay.HideSuggestions();
                    break;
            }
        }
    }
}