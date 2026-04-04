using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Presentation.ConsoleClient
{
    internal class SuggestionDisplay : ISuggestionDisplay
    {
        private List<IndexedWord> allIndexedWords;
        private IndexedWord currentIndexedWord;
        private bool lastWordCompleted;
        private UserSettings settings;
        private PopupWindow popupWindow;
        private ConsoleSizeChecker consoleSizeChecker;
        private int shellPromptLine;
        private int currentShellPromptLength;

        public SuggestionDisplay(UserSettings settings)
        {
            this.lastWordCompleted = true;
            this.settings = settings;
            this.consoleSizeChecker = new ConsoleSizeChecker();
            this.popupWindow = new PopupWindow(settings);
            this.shellPromptLine = 0;
            this.currentShellPromptLength = 0;
            this.allIndexedWords = new List<IndexedWord>();
            this.InitDataStructures();
        }

        public void Initialize(int shellPromptLength)
        {
            this.allIndexedWords.Clear();
            this.shellPromptLine = Console.CursorTop;
            this.currentShellPromptLength = shellPromptLength;
            this.InitDataStructures();
        }

        private void InitDataStructures()
        {
            this.allIndexedWords = new List<IndexedWord>();
            this.currentIndexedWord = new IndexedWord(0, Console.CursorTop, Word.Empty);
            this.currentIndexedWord.offset = this.currentShellPromptLength;
            this.allIndexedWords.Add(currentIndexedWord);
        }

        public void ShowSuggestions(SuggestionViewModel model)
        {
            if (!this.lastWordCompleted)
                popupWindow.Hide();

            ShowSuggestionsInternal(model.CurrentWord, model.IsCorrect, ref currentIndexedWord, model.Suggestions.ToArray());
        }

        public void HideSuggestions()
        {
            popupWindow.Hide();
        }

        public bool IsCurrentlyVisible()
        {
            return popupWindow.IsShown();
        }

        public void SelectNextSuggestion()
        {
            popupWindow.SelectNext();
        }

        public void SelectPreviousSuggestion()
        {
            popupWindow.SelectPrevious();
        }

        public Word CompleteCurrentlySelectedSuggestion()
        {
            Word completion = popupWindow.GetSelectedItem();

            HideSuggestions();
            ReplaceWord(this.currentIndexedWord, completion, this.settings.ValidWordForeColor, this.settings.ValidWordBackColor);
            
            this.currentIndexedWord.word = completion;
            this.lastWordCompleted = true;
            popupWindow.SetSelectedLine(0);

            return completion;
        }

        private void ShowSuggestionsInternal(Word currentWord, bool isCurrentWordInTree, ref IndexedWord currentTypedWord, Word[] suggestions)
        {
            this.lastWordCompleted = false;
            ConsoleColor oldFC = Console.ForegroundColor;
            ConsoleColor oldBC = Console.BackgroundColor;
            ConsoleColor foreColor = isCurrentWordInTree ? this.settings.ValidWordForeColor : this.settings.InvalidWordForeColor;
            ConsoleColor backColor = isCurrentWordInTree ? this.settings.ValidWordBackColor : this.settings.InvalidWordBackColor;

            ReplaceWord(currentTypedWord, currentWord, foreColor, backColor);
            currentTypedWord.word = currentWord;


            if (suggestions == null || suggestions.Length == 0)
                return;

            if (currentWord.ToString() == "")
            {
                this.lastWordCompleted = true;
                return;
            }

        tryShowPopupWindow:

            if (!EnsureEnoughVerticalSpaceInShell(popupWindow.GetPosition().height + 1))
                return;

            //show suggestion popup window
            PopupWindowDisplayState result = popupWindow.Show(currentWord, suggestions);
            switch (result)
            {
                case PopupWindowDisplayState.SUCCESS: 
                    break;
                case PopupWindowDisplayState.NOT_ENOUGH_HORIZONTAL_SPACE: 
                    HandleLinebreak(ref currentTypedWord, foreColor, backColor); 
                    goto tryShowPopupWindow;
            }

            Console.ForegroundColor = oldFC;
            Console.BackgroundColor = oldBC;
        }

        private bool EnsureEnoughVerticalSpaceInShell(int requestedRows)
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

        private void HandleLinebreak(ref IndexedWord currentTypedWord, ConsoleColor foreColor, ConsoleColor backColor)
        {
            Console.SetCursorPosition(this.currentShellPromptLength + currentTypedWord.startIndex, currentTypedWord.line);
            Console.Write(new string(' ', currentTypedWord.word.Length + 1));
            Console.SetCursorPosition(0, ++currentTypedWord.line);
            currentTypedWord.offset = 0;
            currentTypedWord.startIndex = 0;
            ReplaceWord(currentTypedWord, currentTypedWord.word, foreColor, backColor);
        }

        private void ReplaceWord(IndexedWord oldWord, Word newWord, ConsoleColor foreColor, ConsoleColor backColor)
        {
            int oldWordLength = oldWord.word.Length;
            oldWord.word = newWord;

            ConsoleColor oldFC = Console.ForegroundColor;
            ConsoleColor oldBC = Console.BackgroundColor;
            Console.ForegroundColor = foreColor;
            Console.BackgroundColor = backColor;

            Console.SetCursorPosition(oldWord.offset + oldWord.startIndex, oldWord.line);

            string printFormat = this.settings.EnableCapitalizationInInput ? newWord.GetOriginalWordFormat() : newWord.ToString();
            Console.Write(printFormat);

            Console.ForegroundColor = oldFC;
            Console.BackgroundColor = oldBC;

            //clear remains of old word, if it was longer than the new one
            int difference = oldWordLength - newWord.Length;
            if (difference > 0)
            {
                Console.Write(new string(' ', difference));
                Console.CursorLeft -= difference;
            }
        }

        public void NextWord()
        {
            this.lastWordCompleted = true;
            this.allIndexedWords.Add(this.currentIndexedWord);
            this.currentIndexedWord.whiteSpaceAtEnd++;

            int currentWordStartIndex = currentIndexedWord.startIndex;
            int currentWordLength = currentIndexedWord.word.Length;
            int currentWordWhiteSpaceAtEnd = currentIndexedWord.whiteSpaceAtEnd;

            this.currentIndexedWord = new IndexedWord(currentWordStartIndex + currentWordLength + currentWordWhiteSpaceAtEnd, Console.CursorTop, Word.Empty);
            this.currentIndexedWord.offset = this.currentIndexedWord.line == this.shellPromptLine ? this.currentShellPromptLength : 0;
        }

        public void PreviousWord()
        {
            var prev = allIndexedWords.Last();
            allIndexedWords.Remove(prev);
            this.currentIndexedWord = prev;
            this.currentIndexedWord.offset = this.currentIndexedWord.line == this.shellPromptLine ? this.currentShellPromptLength : 0;
        }
    }
}
