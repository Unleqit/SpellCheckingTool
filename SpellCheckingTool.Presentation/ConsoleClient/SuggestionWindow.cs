using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Presentation.ConsoleClient;

public class SuggestionWindow : ISuggestionDisplay
{
    ConsoleColor originalForeColor;
    ConsoleColor originalBackColor;

    private IReadOnlyList<Word> currentSuggestions = Array.Empty<Word>();
    private int currentlySelectedLine = 0;
    private bool suggestionsShown = false;
    private readonly ConsoleSizeChecker consoleSizeChecker;

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

    public int CurrentlySelectedLine
    {
        get => currentlySelectedLine;
        set
        {
            int maxIndex = Math.Max(0, currentSuggestions.Count - 1);
            currentlySelectedLine = Math.Clamp(value, 0, maxIndex);
        }
    }

    public Word CurrentlySelectedSuggestion
     {
         get
         {
            if (currentSuggestions == null ||
             currentlySelectedLine < 0 ||
             currentlySelectedLine >= currentSuggestions.Count)
               return null;

            return currentSuggestions[currentlySelectedLine];
         }
     }

    public SuggestionWindow()
    {
        originalForeColor = Console.ForegroundColor;
        originalBackColor = Console.BackgroundColor;
        consoleSizeChecker = new ConsoleSizeChecker();
    }

    private int GetStartIndexOfCurrentWord(string input)
    {
        return input.LastIndexOf(' ') + 1;
    }

    public void Show(SuggestionViewModel viewModel, int startIndex)
    {
        HideSuggestions();

        currentSuggestions = viewModel.Suggestions;
        currentlySelectedLine = 0; 

        ColorWord(viewModel.CurrentWord, startIndex, viewModel.IsCorrect);

        if (currentSuggestions.Count > 0)
        {
            ShowSuggestions();
        }
    }


    void ReplaceWord(Word lastWord, int startIndexOfCurrentLastWord)
    {
        int currentCursorPos = Console.CursorLeft;
        Console.CursorLeft = startIndexOfCurrentLastWord;

        Console.Write(lastWord);

        //replace any old chars on deletion
        if (currentCursorPos - lastWord.Length > 0)
        {
            Console.Write(new string(' ', currentCursorPos - lastWord.Length));
            Console.CursorLeft = startIndexOfCurrentLastWord + lastWord.Length;
        }
    }

    void ColorWord(Word word, int startIndex, bool isCorrect)
    {
        Console.BackgroundColor = isCorrect ? ValidWordBackColor : InvalidWordBackColor;
        Console.ForegroundColor = isCorrect ? ValidWordForeColor : InvalidWordForeColor;

        ReplaceWord(word, startIndex);

        Console.BackgroundColor = originalBackColor;
        Console.ForegroundColor = originalForeColor;
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

    private int GetDisplayWidth(IReadOnlyList<Word> suggestions, int count)
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
        if (currentSuggestions.Count == 0)
            return;

        suggestionsShown = true;

        int suggestionWindowHeight = currentSuggestions.Count;

        int wordLeftInConsole = Console.CursorLeft;
        int wordTopInConsole = Console.CursorTop;

        if (!CheckAvailableConsoleWindowSpace(suggestionWindowHeight))
            return;

        int displayWidth = GetDisplayWidth(currentSuggestions, suggestionWindowHeight);

        for (int j = 0; j < suggestionWindowHeight; ++j)
        {
            Console.SetCursorPosition(wordLeftInConsole, wordTopInConsole + j + 1);
            Console.BackgroundColor = (j == currentlySelectedLine) ? CurrentlySelectedSuggestionBackColor : SuggestionBackColor;
            Console.ForegroundColor = (j == currentlySelectedLine) ? CurrentlySelectedSuggestionForeColor : SuggestionForeColor;

            WriteSuggestionLine(currentSuggestions[j], displayWidth);
        }

        //restore cursor to old position and set old console colors
        Console.SetCursorPosition(wordLeftInConsole, wordTopInConsole);
        Console.ForegroundColor = originalForeColor;
        Console.BackgroundColor = originalBackColor;
    }

    public void HideSuggestions()
    {
        suggestionsShown = false;

        int suggestionWindowHeight = currentSuggestions.Count;
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
            int startIndex = cursorLeft - 1 < 0 ? 0 : cursorLeft - 1;
            Console.SetCursorPosition(startIndex, wordTopInConsole + j + 1);
            Console.Write(new string(' ', 200)); // simple clear; we’ll size it properly later
        }

        //restore cursor to old positions
        Console.SetCursorPosition(cursorLeft, wordTopInConsole);
    }

    public void AutoCompleteCurrentlySelectedSuggestion(ref string input)
    {
        Word selectedSuggestion = CurrentlySelectedSuggestion;

        if (selectedSuggestion == null) return;

        HideSuggestions();

        int startIndexOfCurrentLastWord = GetStartIndexOfCurrentWord(input);
        Console.CursorLeft = startIndexOfCurrentLastWord;

        Console.ForegroundColor = ValidWordForeColor;
        Console.BackgroundColor = ValidWordBackColor;

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
        currentlySelectedLine = (currentlySelectedLine + 1) % currentSuggestions.Count;
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
            currentlySelectedLine = currentSuggestions.Count - 1;

        ShowSuggestions();
    }

    public bool IsCurrentlyVisible()
    {
        return suggestionsShown;
    }
}