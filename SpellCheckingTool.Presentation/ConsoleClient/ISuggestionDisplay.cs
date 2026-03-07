
using SpellCheckingTool.Application.Suggestion;

namespace SpellCheckingTool.Presentation.ConsoleClient;

public interface ISuggestionDisplay
{
    void Show(SuggestionViewModel model);
    void HideSuggestions();
    void AutoCompleteCurrentlySelectedSuggestion(ref string input);
    void SelectPreviousSuggestion();
    void SelectNextSuggestion();
    bool IsCurrentlyVisible();
}
delegate void WordTreeWordBufferLengthChangedEventHandler(object sender, int newSize);
