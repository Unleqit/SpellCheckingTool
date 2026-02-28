using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool.Presentation.Client;
    public interface ISuggestionDisplay
    {
        public void ShowSuggestionsForString(ref string input);
        public void hideSuggestions();
        public void autoCompleteCurrentlySelectedSuggestion(ref string input);
        public void selectPreviousSuggestion();
        public void selectNextSuggestion();
        public bool isCurrentlyVisible();
    }
    delegate void WordTreeWordBufferLengthChangedEventHandler(object sender, int newSize);
