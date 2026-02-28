using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.SuggestionService;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Presentation.Client;

public unsafe class SuggestionWindow : ISuggestionDisplay
{
    //-------public properties--------

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

    //-------private properties--------

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

        colorWord(lastWordInInput, startIndexOfCurrentLastWord);
        hideSuggestions();
        getSuggestions(lastWordInInput);
        showSuggestions();
    }

    bool checkIfTreeContainsWord(Word word)
    {
        return spellcheckService.IsCorrect(word);
    }

    void replaceWord(Word lastWord, int startIndexOfCurrentLastWord)
    {
        int lengthOfWordToBeReplaced = Console.CursorLeft;
        Console.CursorLeft = startIndexOfCurrentLastWord;
        Console.Write(lastWord);

        //replace any old chars on deletion
        if (lengthOfWordToBeReplaced - lastWord.Length > 0)
        {
            Console.Write(new string(' ', lengthOfWordToBeReplaced - lastWord.Length));
            Console.CursorLeft = startIndexOfCurrentLastWord + lastWord.Length;
        }
    }

    void colorWord(Word lastWord, int startIndexOfCurrentLastWord)
    {
        bool isWordInTree = checkIfTreeContainsWord(lastWord);

        Console.BackgroundColor = isWordInTree ? ValidWordBackColor : InvalidWordBackColor;
        Console.ForegroundColor = isWordInTree ? ValidWordForeColor : InvalidWordForeColor;

        replaceWord(lastWord, startIndexOfCurrentLastWord);

        Console.BackgroundColor = originalBackColor;
        Console.ForegroundColor = originalForeColor;
    }

    void getSuggestions(Word input)
    {
        // TEMP: suggestions are still disabled inside SpellcheckService for now.
        this.currentSuggestions = spellcheckService.GetSuggestions(input, MaxSuggestionsToBeDisplayed, this.SuggestionAlgorithmMaxAllowedDistance);

        int suggestionResultCount = this.currentSuggestions.GetSuggestionCount();
        currentSuggestionCount =
            suggestionResultCount < MaxSuggestionsToBeDisplayed
                ? suggestionResultCount
                : currentSuggestionCount > MaxSuggestionsToBeDisplayed
                    ? MaxSuggestionsToBeDisplayed
                    : suggestionResultCount;
    }

    bool CheckAvailableConsoleWindowSpace(int requestedColumns)
    {
        if (consoleSizeChecker.IsConsoleWindowTooSmall(requestedColumns))
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

    void showSuggestions()
    {
        suggestionsShown = true;

        // we still use WordBufferLength through the result display sizing.
        // Later we can expose this via the use-case if needed.
        int suggestionWindowWidth = 0; // not used directly below, but kept for readability
        int suggestionWindowHeight = currentSuggestionCount;

        int wordLeftInConsole = Console.CursorLeft;
        int wordTopInConsole = Console.CursorTop;

        Word[] suggestions = this.currentSuggestions.GetSuggestionArray();

        if (!CheckAvailableConsoleWindowSpace(suggestionWindowHeight))
            return;

        for (int j = 0; j < suggestionWindowHeight; ++j)
        {
            Console.SetCursorPosition(wordLeftInConsole, Console.CursorTop + 1);
            Console.BackgroundColor = (j == currentlySelectedLine) ? CurrentlySelectedSuggestionBackColor : SuggestionBackColor;
            Console.ForegroundColor = (j == currentlySelectedLine) ? CurrentlySelectedSuggestionForeColor : SuggestionForeColor;

            // keep padding logic, but avoid relying on WordTree meta directly
            Console.Write(new string(' ', horizontalPaddingSz) + suggestions[j] + new string(' ', horizontalPaddingSz));
        }

        //restore cursor to old position and set old console colors
        Console.SetCursorPosition(wordLeftInConsole, wordTopInConsole);
        Console.ForegroundColor = originalForeColor;
        Console.BackgroundColor = originalBackColor;
    }

    public void hideSuggestions()
    {
        suggestionsShown = false;

        int suggestionWindowHeight = CurrentSuggestionCount;
        int cursorLeft = Console.CursorLeft;
        int wordTopInConsole = Console.CursorTop;

        if (!CheckAvailableConsoleWindowSpace(suggestionWindowHeight))
            return;

        //set colors
        Console.BackgroundColor = originalBackColor;
        Console.ForegroundColor = originalForeColor;

        //clear floating suggestions window
        for (int j = 0; j < suggestionWindowHeight; ++j)
        {
            //set cursor pos
            int startIndex = cursorLeft - 1 < 0 ? 0 : cursorLeft - 1;
            Console.SetCursorPosition(startIndex, wordTopInConsole + j + 1);
            Console.Write(new string(' ', 200)); // simple clear; we’ll size it properly later
        }

        //restore cursor to old positions
        Console.SetCursorPosition(cursorLeft, wordTopInConsole);
    }

    public void autoCompleteCurrentlySelectedSuggestion(ref string input)
    {
        hideSuggestions();

        int startIndexOfCurrentLastWord = GetStartIndexOfCurrentWord(input);
        Console.CursorLeft = startIndexOfCurrentLastWord;

        Console.ForegroundColor = ValidWordForeColor;
        Console.BackgroundColor = ValidWordBackColor;

        Word selectedSuggestion = CurrentlySelectedSuggestion;

        replaceWord(selectedSuggestion, startIndexOfCurrentLastWord);

        Console.ForegroundColor = originalForeColor;
        Console.BackgroundColor = originalBackColor;

        input = input.Substring(0, startIndexOfCurrentLastWord) + selectedSuggestion;
    }

    public void selectNextSuggestion()
    {
        if (!suggestionsShown)
            return;

        hideSuggestions();
        currentlySelectedLine = (currentlySelectedLine + 1) % currentSuggestionCount;
        showSuggestions();
    }

    public void selectPreviousSuggestion()
    {
        if (!suggestionsShown)
            return;

        hideSuggestions();

        if (currentlySelectedLine > 0)
            currentlySelectedLine--;
        else
            currentlySelectedLine = currentSuggestionCount - 1;

        showSuggestions();
    }

    public bool isCurrentlyVisible()
    {
        return suggestionsShown;
    }
}