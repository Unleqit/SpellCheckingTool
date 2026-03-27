using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Presentation.ConsoleClient
{
    internal enum PopupWindowDisplayState
    {
        SUCCESS = 0,
        NOT_ENOUGH_VERTICAL_SPACE = 1,
        NOT_ENOUGH_HORIZONTAL_SPACE = 2
    }

    internal class PopupWindow
    {
        private UserSettings settings;
        private Word[] suggestions;
        private PopupWindowCoords position;
        private int selectedLine;
        private bool isShown;
        private ConsoleColor oldBC;
        private ConsoleColor oldFC;
        private Word currentInput;

        public PopupWindow(UserSettings settings)
        {
            this.settings = settings;
            this.position = new PopupWindowCoords();
            this.suggestions = [];
            this.selectedLine = 0;
            this.isShown = false;
            this.oldBC = Console.BackgroundColor;
            this.oldFC = Console.ForegroundColor;
            this.currentInput = Word.Empty;
        }

        public PopupWindowCoords GetPosition()
        {
            return this.position;
        }

        public PopupWindowCoords SetPosition(PopupWindowCoords position)
        {
            var old = position;
            this.position = position;
            return old;
        }

        public Word GetSelectedItem()
        {
            Word selectedItem = this.suggestions[this.selectedLine];
            string selectedItemString = AdjustSuggestionToRespectCapitalizationSetting(this.currentInput, selectedItem);
            
            if (selectedItem.ToString() != selectedItemString)
                selectedItem = new Word(new CustomAlphabet(selectedItemString.ToLower().Distinct().ToArray()), selectedItemString);
           
            return this.isShown ? selectedItem : Word.Empty;
        }

        public int GetSelectedLine()
        {
            return this.isShown ? this.selectedLine : -1;
        }

        public bool IsShown()
        {
            return isShown;
        }

        private PopupWindowDisplayState HasEnoughHorizontalSpaceAtNewPosition(PopupWindowCoords newPosition)
        {
            return (newPosition.left + newPosition.width) <= Console.WindowWidth ? PopupWindowDisplayState.SUCCESS : PopupWindowDisplayState.NOT_ENOUGH_HORIZONTAL_SPACE;
        }

        private PopupWindowDisplayState HasEnoughVerticalSpaceAtNewPosition(PopupWindowCoords newPosition)
        {
            return (newPosition.top + newPosition.height) <= Console.WindowHeight ? PopupWindowDisplayState.SUCCESS : PopupWindowDisplayState.NOT_ENOUGH_VERTICAL_SPACE;
        }

        public PopupWindowDisplayState Show(Word input, Word[] suggestions)
        {
            int maxItemLength = (suggestions.Count() == 0 ? 0 : suggestions.Max((item) => item.Length));

            PopupWindowCoords popupWindowCoords = new PopupWindowCoords();
            popupWindowCoords.left = Console.CursorLeft;
            popupWindowCoords.top = Console.CursorTop + 1;
            popupWindowCoords.width = 2 * this.settings.HorizontalPadding + maxItemLength;
            popupWindowCoords.height = suggestions.Count();
            this.position = popupWindowCoords;

            return Show(input, suggestions, popupWindowCoords);
        }

        public PopupWindowDisplayState Show(Word input, Word[] suggestions, PopupWindowCoords popupWindowCoords)
        {
            PopupWindowDisplayState result = HasEnoughHorizontalSpaceAtNewPosition(popupWindowCoords) | HasEnoughVerticalSpaceAtNewPosition(popupWindowCoords);
            if (result != PopupWindowDisplayState.SUCCESS)
                return result;

            this.isShown = true;
            this.position = popupWindowCoords;
            this.suggestions = suggestions;
            this.currentInput = input;

            int oldCursorLeft = Console.CursorLeft;
            int oldCursorTop = Console.CursorTop;

            for (int j = 0; j < position.height; ++j)
            {
                Console.SetCursorPosition(position.left, position.top + j);
                Console.BackgroundColor = (j == this.selectedLine) ? this.settings.SelectedSuggestionBackColor : this.settings.SuggestionBackColor;
                Console.ForegroundColor = (j == this.selectedLine) ? this.settings.SelectedSuggestionForeColor : this.settings.SuggestionForeColor;

                WriteSuggestionLine(input, suggestions[j], position.width);
            }

            Console.BackgroundColor = oldBC;
            Console.ForegroundColor = oldFC;
            Console.SetCursorPosition(oldCursorLeft, oldCursorTop);

            return PopupWindowDisplayState.SUCCESS;
        }

        private void WriteSuggestionLine(Word input, Word suggestion, int suggestionPopupWindowWidth)
        {
            int rightFill = suggestionPopupWindowWidth - 2 * this.settings.HorizontalPadding - suggestion.Length;
            rightFill = rightFill < 0 ? 0 : rightFill;

            string suggestionString = AdjustSuggestionToRespectCapitalizationSetting(input, suggestion);
            string paddedSuggestion = new string(' ', this.settings.HorizontalPadding) + suggestionString + new string(' ', rightFill + this.settings.HorizontalPadding);
            Console.Write(paddedSuggestion);
        }

        private string AdjustSuggestionToRespectCapitalizationSetting(Word input, Word suggestion)
        {
            string suggestionString = suggestion.ToString();
            string inputString = input.ToString();
            if (this.settings.EnableCapitalizationInInput && this.settings.AdjustCapitalizationInSuggestions && suggestionString.StartsWith(inputString))
                suggestionString = input.GetOriginalWordFormat() + suggestionString.Substring(inputString.Length);
            return suggestionString;
        }

        public void Hide()
        {
            if (!this.isShown)
                return;

            this.isShown = false;
            string clearEntry = new string(' ', position.width);
            int oldCursorLeft = Console.CursorLeft;
            int oldCursorTop = Console.CursorTop;
            int startLeft = position.left;
            int startTop = position.top;

            Console.BackgroundColor = oldBC;
            Console.ForegroundColor = oldFC;

            for (int j = 0; j < position.height; ++j)
            {
                Console.SetCursorPosition(startLeft, startTop + j);
                Console.Write(clearEntry);
            }

            Console.SetCursorPosition(oldCursorLeft, oldCursorTop);
        }

        public void SelectNext()
        {
            int newLine = this.selectedLine;
            newLine = (newLine + 1) % this.suggestions.Length;
            SetSelectedLine(newLine);
        }

        public void SelectPrevious()
        {
            int newLine = this.selectedLine;
            newLine = newLine > 0 ? newLine - 1 : this.suggestions.Length - 1;
            SetSelectedLine(newLine);
        }

        public int SetSelectedLine(int line)
        {
            if (!isShown)
                return this.selectedLine = line;

            Hide();
            int oldLine = this.selectedLine;
            this.selectedLine = line;
            Show(this.currentInput, this.suggestions, this.position);
            return oldLine;
        }
    }
}
