
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Presentation.ConsoleClient;

public interface ISuggestionDisplay
{
    void Show(SuggestionViewModel model);
    void HideSuggestions();
    Word CompleteCurrentlySelectedSuggestion();
    void SelectPreviousSuggestion();
    void SelectNextSuggestion();
    bool IsCurrentlyVisible();
}
