using SpellCheckingTool.Domain;
using SpellCheckingTool.Domain.Suggestion;

namespace SpellCheckingTool.Application.Suggestion.SuggestionService
{
    public class SuggestionService : SuggestionServiceBase
    {
        public SuggestionService(IWordStorage tree, IDistanceAlgorithm distanceAlgorithm) 
            : base(tree, distanceAlgorithm)
        {
            base.onPreWalk = null;
            base.computeNormalizedDistance = ComputeNormalizedDistance;
        }

        protected static double ComputeNormalizedDistance(Word input, Word word, 
            IDistanceAlgorithm distanceAlgorithm, int maxAllowedDistance)
        {
            double distanceToInputWord = distanceAlgorithm.GetDistance(input, word);
            if (distanceToInputWord > maxAllowedDistance)
                return -1;

            int worstPossibleDistance = Math.Max(input.Length, word.Length);
            double normalizedDistance = distanceToInputWord / worstPossibleDistance;
            return normalizedDistance;
        }
    }
}
