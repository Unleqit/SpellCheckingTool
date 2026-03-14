using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordTree;
using System.Drawing;
using System.Reflection;

namespace SpellCheckingTool.Presentation.ConsoleClient;

public class SuggestionWindow : ISuggestionDisplay
{
    ConsoleColor originalForeColor;
    ConsoleColor originalBackColor;

    private IReadOnlyList<Word> currentSuggestions = Array.Empty<Word>();
    private int currentlySelectedLine = 0;

    private bool suggestionsShown = false;
    private readonly ConsoleSizeChecker consoleSizeChecker;
    private int offset = 0;

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

    public Word? CurrentlySelectedSuggestion
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

    private Word oldWord = new Word(new CustomAlphabet(new char[] { ' ' }), "");

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

    private SuggestionViewModel currentSuggestionViewModel;
    public void Show(SuggestionViewModel viewModel)
    {
        this.currentSuggestionViewModel = viewModel;
        HideSuggestions();

        currentSuggestions = viewModel.Suggestions;
        currentlySelectedLine = 0;

        offset = viewModel.Offset;
        ColorWord(viewModel.CurrentWord, this.oldWord, viewModel.StartIndex, viewModel.IsCorrect, offset);

        if (viewModel.CurrentWord.ToString().Length > 0)
        {
            ShowSuggestions();
            oldWord = viewModel.CurrentWord;
        }
    }

    void ReplaceWord(Word wordToReplace, Word oldWord, int oldWordStartIndex, int shellOffset = 0)
    {
        int lengthOfWordToBeReplaced = Console.CursorLeft - shellOffset;
        Console.CursorLeft = shellOffset + oldWordStartIndex;

        Console.Write(wordToReplace);

        int difference = oldWord.Length - wordToReplace.Length;
        if (difference > 0)
        {
            Console.Write(new string(' ', difference));
            Console.CursorLeft -= difference;
        }
    }

    void ColorWord(Word lastWord, Word oldWord, int startIndexOfCurrentLastWord, bool isCorrect, int offset = 0)
    {
        Console.BackgroundColor = isCorrect ? ValidWordBackColor : InvalidWordBackColor;
        Console.ForegroundColor = isCorrect ? ValidWordForeColor : InvalidWordForeColor;

        ReplaceWord(lastWord, oldWord, startIndexOfCurrentLastWord, offset);

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

    private bool linebreak = false;

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

        //check if newline is required
        int oldLeft = Console.CursorLeft;
        if ((Console.CursorLeft + 2 * horizontalPaddingSz + (currentSuggestions.Count() == 0 ? 0 : currentSuggestions.Max((item) => item.Length))) > Console.WindowWidth)
        {
            //remove beginning of currently typed word in old line
            Console.CursorLeft -= oldWord.Length + 1;
            Console.Write(new string(' ', oldWord.Length + 1));

            //write current word to new line and show suggestions for it
            Console.CursorTop++;
            Console.CursorLeft = currentSuggestionViewModel.CurrentWord.Length;
            wordTopInConsole = Console.CursorTop;
            wordLeftInConsole = Console.CursorLeft;
            offset = 0;

            if (!linebreak)
            {
                linebreak = true;
                ColorWord(currentSuggestionViewModel.CurrentWord, currentSuggestionViewModel.CurrentWord, 0, currentSuggestionViewModel.IsCorrect);
            }
            else
                return;
        }


        for (int j = 0; j < suggestionWindowHeight; ++j)
        {
            Console.SetCursorPosition(wordLeftInConsole, Console.CursorTop + 1);
            Console.BackgroundColor = (j == currentlySelectedLine) ? CurrentlySelectedSuggestionBackColor : SuggestionBackColor;
            Console.ForegroundColor = (j == currentlySelectedLine) ? CurrentlySelectedSuggestionForeColor : SuggestionForeColor;

            WriteSuggestionLine(currentSuggestions[j], displayWidth);
        }

        Console.SetCursorPosition(wordLeftInConsole, wordTopInConsole);
        Console.ForegroundColor = originalForeColor;
        Console.BackgroundColor = originalBackColor;
    }

    public void HideSuggestions()
    {
        if (currentSuggestions == null || currentSuggestions.Count == 0)
            return;

        suggestionsShown = false;

        int suggestionWindowHeight = currentSuggestions.Count;
        int cursorLeft = Console.CursorLeft;
        int wordTopInConsole = Console.CursorTop;

        if (!CheckAvailableConsoleWindowSpace(suggestionWindowHeight))
            return;

        Console.BackgroundColor = originalBackColor;
        Console.ForegroundColor = originalForeColor;

        int displayWidth = GetDisplayWidth(currentSuggestions, suggestionWindowHeight);

        int length = 2 * horizontalPaddingSz + displayWidth + 2;


        for (int j = 0; j < suggestionWindowHeight; ++j)
        {
            int startIndex = cursorLeft - 1 < 0 ? 0 : cursorLeft - 1;
            Console.SetCursorPosition(startIndex, wordTopInConsole + j + 1);
            Console.Write(new string(' ', length));
        }

        Console.SetCursorPosition(cursorLeft, wordTopInConsole);
    }

    public Word CompleteCurrentlySelectedSuggestion()
    {
        Word? selectedSuggestion = CurrentlySelectedSuggestion;

        if (selectedSuggestion == null) 
            return Word.Empty;

        HideSuggestions();

        int startIndexOfCurrentLastWord = currentSuggestionViewModel.StartIndex;
        Console.CursorLeft = offset + startIndexOfCurrentLastWord;

        Console.ForegroundColor = ValidWordForeColor;
        Console.BackgroundColor = ValidWordBackColor;

        ReplaceWord(selectedSuggestion, oldWord, startIndexOfCurrentLastWord, offset);

        Console.ForegroundColor = originalForeColor;
        Console.BackgroundColor = originalBackColor;

        return selectedSuggestion;
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