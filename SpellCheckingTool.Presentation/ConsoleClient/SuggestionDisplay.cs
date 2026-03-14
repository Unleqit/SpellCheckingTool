using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Presentation.ConsoleClient
{
    internal class SuggestionDisplay : ISuggestionDisplay
    {
        internal struct IndexedWord
        {
            public int startIndex;
            public int line;
            public Word word;

            public IndexedWord(int startIndex, int line, Word word)
            {
                this.startIndex = startIndex;
                this.line = line;
                this.word = word;
            }
        }

        private SuggestionViewModel suggestionViewModel;
        private int selectedSuggestionLine;
        private bool suggestionsShown;
        private List<IndexedWord> allIndexedWords;
        private IndexedWord currentIndexedWord;
        private bool lastWordCompleted;
        private int horizontalPaddingSz;

        public SuggestionDisplay(UserSettings settings)
        {
            this.suggestionViewModel = new SuggestionViewModel(Word.Empty, false, [], 0);
            this.selectedSuggestionLine = 0;
            this.suggestionsShown = false;
            this.allIndexedWords = new List<IndexedWord>();
            this.allIndexedWords.Add(new IndexedWord(0, Console.CursorTop, Word.Empty));
            this.currentIndexedWord = new IndexedWord();
            this.lastWordCompleted = true;
            this.horizontalPaddingSz = 0;
        }

        public void Show(SuggestionViewModel model)
        {
            this.suggestionViewModel = model;
            this.suggestionsShown = true;

            //update data structure
            if (this.lastWordCompleted)
                this.currentIndexedWord = new IndexedWord(this.allIndexedWords.Last().startIndex + this.allIndexedWords.Last().word.Length, Console.CursorTop, model.CurrentWord);
            else
                this.currentIndexedWord.word = model.CurrentWord;

            ShowSuggestions();
        }

        public Word CompleteCurrentlySelectedSuggestion()
        {
            Word completion = suggestionViewModel.Suggestions[selectedSuggestionLine];

            //update data structures
            this.currentIndexedWord.word = completion;
            this.allIndexedWords.Add(currentIndexedWord);
            this.lastWordCompleted = true;
            int currentWordStartIndex = currentIndexedWord.startIndex;
            int currentWordLength = currentIndexedWord.word.Length;
            this.currentIndexedWord = new IndexedWord(currentWordStartIndex + currentWordLength, Console.CursorTop, Word.Empty);

            return completion;
        }

        public void HideSuggestions()
        {

            this.suggestionsShown = false;

            throw new NotImplementedException();

        }

        public bool IsCurrentlyVisible()
        {
            return this.suggestionsShown;
        }

        public void SelectNextSuggestion()
        {
            if (!this.suggestionsShown)
                return;

            HideSuggestions();
            this.selectedSuggestionLine = (this.selectedSuggestionLine + 1) % this.suggestionViewModel.Suggestions.Count;
            ShowSuggestions();
        }

        public void SelectPreviousSuggestion()
        {
            if (!suggestionsShown)
                return;

            HideSuggestions();

            if (this.selectedSuggestionLine > 0)
                this.selectedSuggestionLine--;
            else
                this.selectedSuggestionLine = this.suggestionViewModel.Suggestions.Count - 1;

            ShowSuggestions();
        }


        private void ShowSuggestions()
        {

        }



        private void WriteSuggestionLines(int suggestionPopupWindowWidth)
        {
            if (this.suggestionViewModel == null || this.suggestionViewModel.Suggestions == null)
                return;

            for (int j = 0; j < this.suggestionViewModel.Suggestions.Count(); ++j)
            {
                Console.SetCursorPosition(currentIndexedWord.startIndex + , Console.CursorTop + 1);
                Console.BackgroundColor = (j == this.selectedSuggestionLine) ? CurrentlySelectedSuggestionBackColor : this.selectedSuggestionLine;
                Console.ForegroundColor = (j == this.selectedSuggestionLine) ? CurrentlySelectedSuggestionForeColor : this.selectedSuggestionLine;

                WriteSuggestionLine(currentSuggestions[j], displayWidth);
            }
        }

        private void WriteSuggestionLine(Word suggestion, int suggestionPopupWindowWidth)
        {
            int rightFill = suggestionPopupWindowWidth - suggestion.Length;
            if (rightFill < 0) rightFill = 0;

            Console.Write(new string(' ', horizontalPaddingSz));
            Console.Write(suggestion.ToString());
            Console.Write(new string(' ', rightFill + horizontalPaddingSz));
        }

        private void ColorWord(IndexedWord word, ConsoleColor foreColor, ConsoleColor backColor)
        {
            ConsoleColor oldFC = Console.ForegroundColor;
            ConsoleColor oldBC = Console.BackgroundColor;
            Console.ForegroundColor = foreColor;
            Console.BackgroundColor = backColor;

            Console.ForegroundColor = oldFC;
            Console.BackgroundColor = oldBC;
        }
    }
}
