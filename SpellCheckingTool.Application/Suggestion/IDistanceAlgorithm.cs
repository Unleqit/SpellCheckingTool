using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Suggestion;

public interface IDistanceAlgorithm
{
    int GetDistance(Word a, Word b);
}