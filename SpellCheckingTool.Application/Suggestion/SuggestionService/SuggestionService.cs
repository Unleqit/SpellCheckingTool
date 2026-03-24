using SpellCheckingTool.Domain.WordTree;
using System;
using System.Collections.Generic;
namespace SpellCheckingTool.Application.Suggestion.SuggestionService
{
    public class SuggestionService : SuggestionServiceBase
    {
        public SuggestionService(WordTree tree, IDistanceAlgorithm distanceAlgorithm) : base(tree, distanceAlgorithm)
        {
        }

        /// <summary>
        /// Returns a value between 0 and 1 representing the normalized distance value of the two passed words.
        /// Retunds -1, if their levenshtein distance exceeds the provided threshold.
        /// </summary>
        protected override double ComputeNormalizedDistance(Word input, Word word, IDistanceAlgorithm distanceAlgorithm, int maxAllowedDistance)
        {
            double distanceToInputWord = distanceAlgorithm.GetDistance(input, word);
            if (distanceToInputWord > maxAllowedDistance)
                return -1;

            //this is a property of the levenshtein algorithm
            int worstPossibleDistance = Math.Max(input.Length, word.Length);
            double normalizedDistance = distanceToInputWord / worstPossibleDistance;
            return normalizedDistance;
        }
    }
}
