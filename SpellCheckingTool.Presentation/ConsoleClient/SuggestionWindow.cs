using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Presentation.ConsoleClient;

public class SuggestionWindow : ISuggestionDisplay
{
    public int MaxSuggestionsToBeDisplayed { get; set; }

    int horizontalPaddingSz;
    public int HorizontalPaddingSz
    {
        get => horizontalPaddingSz;
        set => horizontalPaddingSz = Math.Clamp(value, 0, 10);
    }

    public ConsoleColor ValidWordForeColor { get; set; }
    public ConsoleColor ValidWordBackColor { get; set; }
    public ConsoleColor InvalidWordForeColor { get; set; }
    public ConsoleColor InvalidWordBackColor { get; set; }
    public ConsoleColor SuggestionForeColor { get; set; }
    public ConsoleColor SuggestionBackColor { get; set; }
    public ConsoleColor CurrentlySelectedSuggestionForeColor { get; set; }
    public ConsoleColor CurrentlySelectedSuggestionBackColor { get; set; }

    int currentSuggestionCount;
    public int CurrentSuggestionCount { get => currentSuggestionCount; }

    int suggestionAlgorithmMaxAllowedDistance;
    public int SuggestionAlgorithmMaxAllowedDistance
    {
        get => suggestionAlgorithmMaxAllowedDistance;
        set => suggestionAlgorithmMaxAllowedDistance = value < 0 ? 0 : value;
    }

    int currentlySelectedLine;
    public int CurrentlySelectedLine
    {
        get => currentlySelectedLine;
        set => currentlySelectedLine = value < 0 ? 0 : value > currentSuggestionCount ? currentSuggestionCount : value;
    }

    public Word CurrentlySelectedSuggestion
    {
        get
        {
            if (currentlySelectedLine >= currentSuggestions.GetSuggestionCount() || currentlySelectedLine < 0)
                return new Word(spellcheckService.Alphabet, "");

            Word[] suggestions = currentSuggestions.GetSuggestionArray();
            return suggestions[currentlySelectedLine];
        }
    }

    ConsoleColor originalForeColor;
    ConsoleColor originalBackColor;
    bool suggestionsShown = true;

    private readonly ISpellcheckService spellcheckService;

    SuggestionResult currentSuggestions;
    ConsoleSizeChecker consoleSizeChecker;

    public SuggestionWindow(ISpellcheckService spellcheckService)
    {
        this.spellcheckService = spellcheckService;
        this.originalForeColor = Console.ForegroundColor;
        this.originalBackColor = Console.BackgroundColor;
        this.consoleSizeChecker = new ConsoleSizeChecker();
    }

    private int GetStartIndexOfCurrentWord(string input)
    {
        return input.LastIndexOf(' ') + 1;
    }

    private Word GetLastWordFromInputString(string input)
    {
        int startIndexOfCurrentWord = GetStartIndexOfCurrentWord(input);
        string wordString = input.Substring(startIndexOfCurrentWord);
        return new Word(spellcheckService.Alphabet, wordString);
    }

    public void ShowSuggestionsForString(ref string input)
    {
        Word lastWordInInput = GetLastWordFromInputString(input);
        int startIndexOfCurrentLastWord = GetStartIndexOfCurrentWord(input);

        ColorWord(lastWordInInput, startIndexOfCurrentLastWord);
        HideSuggestions();
        GetSuggestions(lastWordInInput);
        ShowSuggestions();
    }

    bool CheckIfTreeContainsWord(Word word)
    {
        return spellcheckService.IsCorrect(word);
    }

    void ReplaceWord(Word lastWord, int startIndexOfCurrentLastWord)
    {
        int lengthOfWordToBeReplaced = Console.CursorLeft;
        Console.CursorLeft = startIndexOfCurrentLastWord;
        Console.Write(lastWord);

        if (lengthOfWordToBeReplaced - lastWord.Length > 0)
        {
            Console.Write(new string(' ', lengthOfWordToBeReplaced - lastWord.Length));
            Console.CursorLeft = startIndexOfCurrentLastWord + lastWord.Length;
        }
    }

    void ColorWord(Word lastWord, int startIndexOfCurrentLastWord)
    {
        bool isWordInTree = CheckIfTreeContainsWord(lastWord);

        Console.BackgroundColor = isWordInTree ? ValidWordBackColor : InvalidWordBackColor;
        Console.ForegroundColor = isWordInTree ? ValidWordForeColor : InvalidWordForeColor;

        ReplaceWord(lastWord, startIndexOfCurrentLastWord);

        Console.BackgroundColor = originalBackColor;
        Console.ForegroundColor = originalForeColor;
    }

    void GetSuggestions(Word input)
    {
        this.currentSuggestions = spellcheckService.GetSuggestions(input, MaxSuggestionsToBeDisplayed, this.SuggestionAlgorithmMaxAllowedDistance);

        int suggestionResultCount = this.currentSuggestions.GetSuggestionCount();
        currentSuggestionCount =
            suggestionResultCount < MaxSuggestionsToBeDisplayed
                ? suggestionResultCount
                : currentSuggestionCount > MaxSuggestionsToBeDisplayed
                    ? MaxSuggestionsToBeDisplayed
                    : suggestionResultCount;
    }

    bool CheckAvailableConsoleWindowSpace(int requestedRows)
    {
        if (consoleSizeChecker.IsConsoleWindowTooSmall(requestedRows))
        {
            if (!consoleSizeChecker.IsErrorMessageDisplayed())
                consoleSizeChecker.ShowConsoleTooSmallError();
            return false;
        }
        else if (consoleSizeChecker.IsErrorMessageDisplayed())
        {
            consoleSizeChecker.HideConsoleTooSmallError();
        }
        return true;
    }

    private int GetDisplayWidth(Word[] suggestions, int count)
    {
        int maxLen = 0;

        for (int i = 0; i < count; i++)
        {
            var w = suggestions[i];
            if (w == null) continue;

            int len = w.Length;
            if (len > maxLen) maxLen = len;
        }

        return Math.Max(1, maxLen);
    }

    private void WriteSuggestionLine(Word suggestion, int displayWidth)
    {
        int rightFill = displayWidth - suggestion.Length;
        if (rightFill < 0) rightFill = 0;

        Console.Write(new string(' ', horizontalPaddingSz));
        Console.Write(suggestion.ToString());
        Console.Write(new string(' ', rightFill + horizontalPaddingSz));
    }

    void ShowSuggestions()
    {
        suggestionsShown = true;

        int suggestionWindowHeight = currentSuggestionCount;

        int wordLeftInConsole = Console.CursorLeft;
        int wordTopInConsole = Console.CursorTop;

        Word[] suggestions = this.currentSuggestions.GetSuggestionArray();

        if (!CheckAvailableConsoleWindowSpace(suggestionWindowHeight))
            return;

        int displayWidth = GetDisplayWidth(suggestions, suggestionWindowHeight);

        for (int j = 0; j < suggestionWindowHeight; ++j)
        {
            Console.SetCursorPosition(wordLeftInConsole, Console.CursorTop + 1);
            Console.BackgroundColor = (j == currentlySelectedLine) ? CurrentlySelectedSuggestionBackColor : SuggestionBackColor;
            Console.ForegroundColor = (j == currentlySelectedLine) ? CurrentlySelectedSuggestionForeColor : SuggestionForeColor;

            WriteSuggestionLine(suggestions[j], displayWidth);
        }

        Console.SetCursorPosition(wordLeftInConsole, wordTopInConsole);
        Console.ForegroundColor = originalForeColor;
        Console.BackgroundColor = originalBackColor;
    }

    public void HideSuggestions()
    {
        suggestionsShown = false;

        int suggestionWindowHeight = CurrentSuggestionCount;
        int cursorLeft = Console.CursorLeft;
        int wordTopInConsole = Console.CursorTop;

        if (!CheckAvailableConsoleWindowSpace(suggestionWindowHeight))
            return;

        Console.BackgroundColor = originalBackColor;
        Console.ForegroundColor = originalForeColor;

        int longestSuggestionLength = 0;
        if (this.currentSuggestions != null && this.currentSuggestions.GetSuggestionCount() > 0)
            longestSuggestionLength = this.currentSuggestions.GetSuggestionArray().Max((suggestion) => suggestion.Length);
        int length = 2 * horizontalPaddingSz + longestSuggestionLength + 2;

        for (int j = 0; j < suggestionWindowHeight; ++j)
        {
            int startIndex = cursorLeft - 1 < 0 ? 0 : cursorLeft - 1;
            Console.SetCursorPosition(startIndex, wordTopInConsole + j + 1);
            Console.Write(new string(' ', length));
        }

        Console.SetCursorPosition(cursorLeft, wordTopInConsole);
    }

    public void AutoCompleteCurrentlySelectedSuggestion(ref string input)
    {
        HideSuggestions();

        int startIndexOfCurrentLastWord = GetStartIndexOfCurrentWord(input);
        Console.CursorLeft = startIndexOfCurrentLastWord;

        Console.ForegroundColor = ValidWordForeColor;
        Console.BackgroundColor = ValidWordBackColor;

        Word selectedSuggestion = CurrentlySelectedSuggestion;

        ReplaceWord(selectedSuggestion, startIndexOfCurrentLastWord);

        Console.ForegroundColor = originalForeColor;
        Console.BackgroundColor = originalBackColor;

        input = input.Substring(0, startIndexOfCurrentLastWord) + selectedSuggestion;
    }

    public void SelectNextSuggestion()
    {
        if (!suggestionsShown)
            return;

        HideSuggestions();
        currentlySelectedLine = (currentlySelectedLine + 1) % currentSuggestionCount;
        ShowSuggestions();
    }

    public void SelectPreviousSuggestion()
    {
        if (!suggestionsShown)
            return;

        HideSuggestions();

        if (currentlySelectedLine > 0)
            currentlySelectedLine--;
        else
            currentlySelectedLine = currentSuggestionCount - 1;

        ShowSuggestions();
    }

    public bool IsCurrentlyVisible()
    {
        return suggestionsShown;
    }
}
