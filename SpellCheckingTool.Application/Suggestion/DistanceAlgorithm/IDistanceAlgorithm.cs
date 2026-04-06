using SpellCheckingTool.Domain;

namespace SpellCheckingTool.Application.Suggestion;

public interface IDistanceAlgorithm
{
    int GetDistance(Word a, Word b);
}