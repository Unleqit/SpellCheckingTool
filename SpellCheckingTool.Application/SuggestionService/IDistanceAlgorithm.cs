using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.SuggestionService;

public interface IDistanceAlgorithm
{
    int GetDistance(Word a, Word b);
}