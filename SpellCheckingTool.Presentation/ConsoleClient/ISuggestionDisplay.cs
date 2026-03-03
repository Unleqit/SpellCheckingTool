
namespace SpellCheckingTool.Presentation.ConsoleClient;

public interface ISuggestionDisplay
{
    void ShowSuggestionsForString(ref string input);
    void HideSuggestions();
    void AutoCompleteCurrentlySelectedSuggestion(ref string input);
    void SelectPreviousSuggestion();
    void SelectNextSuggestion();
    bool IsCurrentlyVisible();
}
delegate void WordTreeWordBufferLengthChangedEventHandler(object sender, int newSize);
