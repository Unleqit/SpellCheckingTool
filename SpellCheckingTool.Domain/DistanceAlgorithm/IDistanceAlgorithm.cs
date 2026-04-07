using SpellCheckingTool.Domain;

namespace SpellCheckingTool.Domain.Suggestion;

public interface IDistanceAlgorithm
{
    int GetDistance(Word a, Word b);
}