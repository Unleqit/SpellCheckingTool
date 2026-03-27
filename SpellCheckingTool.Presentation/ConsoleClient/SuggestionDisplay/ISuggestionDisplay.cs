
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Presentation.ConsoleClient;

public interface ISuggestionDisplay
{
    void ShowSuggestions(SuggestionViewModel model);
    void HideSuggestions();
    Word CompleteCurrentlySelectedSuggestion();
    void SelectPreviousSuggestion();
    void SelectNextSuggestion();
    bool IsCurrentlyVisible();
    void NextWord();
    void PreviousWord();
    void Initialize(int shellPromptLength);
}
