using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Domain.WordTree;
using System.Runtime.CompilerServices;

namespace SpellCheckingTool.Infrastructure.Suggestions;

public class LevenshteinDistanceAlgorithm : IDistanceAlgorithm
{
    /// <summary>
    /// Provides an implementation of the levenshtein distance matching algorithm (see https://en.wikipedia.org/wiki/Levenshtein_distance)
    /// </summary>
    public LevenshteinDistanceAlgorithm()
    {
    }

    /// <summary>
    /// Returns the Levenshtein distance between the two provided words.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public int GetDistance(Word wordA, Word wordB)
    {
        int minimalCost;
        int deletionCost;
        int insertionCost;
        int substitutionCost;

        //Make sure wordB is the shorter string (minimize buffer needs)
        if (wordB.Length > wordA.Length)
        {
            Word tmp = wordA;
            wordA = wordB;
            wordB = tmp;
        }

        int bufferLength = Math.Max(wordA.Length, wordB.Length) + 1;

        int[] prev = new int[bufferLength];
        int[] current = new int[bufferLength];
        int[] _prev = prev;
        int[] _current = current;
        int[] _tmpBuffer;

        //populate the prev array with the cost of deletions
        for (int i = 0; i <= wordB.Length; i++)
            _prev[i] = i;

        //fill current array according to levenshtein rules
        deletionCost = 0;
        insertionCost = 0;
        substitutionCost = 0;

        for (int i = 1; i <= wordA.Length; i++)
        {
            _current[0] = i;

            for (int j = 1; j <= wordB.Length; j++)
            {
                deletionCost = _prev[j] + 1;
                insertionCost = _current[j - 1] + 1;
                substitutionCost = _prev[j - 1] + ((wordA[i - 1] == wordB[j - 1]) ? 0 : 1);

                minimalCost = deletionCost < insertionCost ? deletionCost : insertionCost;
                _current[j] = minimalCost < substitutionCost ? minimalCost : substitutionCost;
            }

            //swap buffers
            _tmpBuffer = _prev;
            _prev = _current;
            _current = _tmpBuffer;
        }

        return _prev[wordB.Length];
    }
}