using SpellCheckingTool.Domain;
using SpellCheckingTool.Domain.Suggestion;

namespace SpellCheckingTool.Application.Suggestion.SuggestionService
{
    public class SuggestionService : SuggestionServiceBase
    {
        private IDistanceAlgorithm distanceAlgorithm;

        public SuggestionService(IWordStorage wordStorage, IDistanceAlgorithm distanceAlgorithm) : base(wordStorage)
        {
            this.distanceAlgorithm = distanceAlgorithm;
        }

        /// <summary>
        /// Returns a value between 0 and 1 representing the normalized distance value of the two passed words.
        /// Retunds -1, if their levenshtein distance exceeds the provided threshold.
        /// </summary>
        protected override double ComputeScore(Word inputWord, Word otherWord, int maxAllowedDistance)
        {
            double distanceToInputWord = this.distanceAlgorithm.GetDistance(inputWord, otherWord);
            if (distanceToInputWord > maxAllowedDistance)
                return -1;

            //this is a property of the levenshtein algorithm
            int worstPossibleDistance = Math.Max(inputWord.Length, otherWord.Length);
            double normalizedDistance = distanceToInputWord / worstPossibleDistance;
            return normalizedDistance;
        }

        protected override void OnPreWalk()
        {

        }
    }
}
