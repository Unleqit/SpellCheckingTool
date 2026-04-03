using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.WordStats;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Suggestion.SuggestionService
{
    public class StatisticSuggestionService : SuggestionServiceBase
    {
        private UserService userService;
        private IEnumerable<WordStatistic> currentStats;
        private Guid guid;
        private int lastNDays;

        public StatisticSuggestionService(WordTree tree, IDistanceAlgorithm distanceAlgorithm, UserService userService, Guid guid, int lastNDays = 14) : base(tree, distanceAlgorithm)
        {
            this.userService = userService;
            this.guid = guid;
            this.lastNDays = lastNDays;
        }

        /// <summary>
        /// Returns a value between 0 and 1 representing the normalized distance value of the two passed words.
        /// Retunds -1, if their levenshtein distance exceeds the provided threshold.
        /// </summary>
        protected override void Prewalk()
        {
            var result = this.userService.GetStats(this.guid);
            if (!result.Success || result.Value == null)
                return;
            var stats = result.Value;
            var offset = DateTime.Now.Subtract(TimeSpan.FromDays(this.lastNDays));
            var statsInTimeFrame = stats.Where((entry) => entry.LastUsedAt > offset);
            this.currentStats = statsInTimeFrame;
        }

        /// <summary>
        /// Returns a value between 0 and 1 representing the normalized distance value of the two passed words.
        /// 1: The word has maximal achievable distance (none of the characters match or are in the right places)
        /// 0: The two words are equal
        /// Retunds -1, if the levenshtein distance of both words exceeds the provided threshold.
        /// </summary>
        private double GetNormalizedDistance(Word input, Word otherWord, IDistanceAlgorithm distanceAlgorithm, int maxAllowedDistance)
        {
            double distanceToInputWord = distanceAlgorithm.GetDistance(input, otherWord);
            if (distanceToInputWord > maxAllowedDistance)
                return -1;

            //this is a property of the levenshtein algorithm
            int worstPossibleDistance = Math.Max(input.Length, otherWord.Length);
            double normalizedDistance = distanceToInputWord / worstPossibleDistance;
            return normalizedDistance;
        }

        /// <summary>
        /// Returns a value between 0 and 1 representing the normalized metric this word was last used.
        /// 1: The word was last used before the specified time period (or never)
        /// 0: The word was last used within the last hour
        /// </summary>
        private double GetNormalizedLastUsedMetric(Word input, int lastNDays = 14)
        {
            //.Max() crashes when the sequence has 0 elements...
            if (this.currentStats == null || this.currentStats.Count() == 0)
                return 0;
            var inputWordStat = this.currentStats.FirstOrDefault((entry) => entry.Word == input);
            if (inputWordStat == null)
                return 0;
            var now = DateTime.Now;
            return (now.Subtract(inputWordStat.LastUsedAt).TotalHours / (lastNDays * 24));
        }

        /// <summary>
        /// Returns a value between 0 and 1 representing the normalized frequency the word was used.
        /// 0: The word was never used before
        /// 1: The word is the most used word of the user.
        /// </summary>
        private double GetNormalizedMostUsedMetric(Word input, int lastNDays = 14)
        {
            //.Max() crashes when the sequence has 0 elements...
            if (this.currentStats == null || this.currentStats.Count() == 0)
                return 1;
            var highestUsageCount = this.currentStats.Max((entry) => entry.UsageCount);
            if (highestUsageCount == 0)
                return 1;
            double inputWordUsageCount = this.currentStats.FirstOrDefault((entry) => entry.Word == input)?.UsageCount ?? 0;
            return 1 - (inputWordUsageCount / highestUsageCount);
        }

        /// <summary>
        /// Returns a value between 0 and 1 representing the normalized distance value of the two passed words.
        /// Retunds -1, if their levenshtein distance exceeds the provided threshold.
        /// </summary>
        protected override double ComputeNormalizedDistance(Word input, Word otherWord, IDistanceAlgorithm distanceAlgorithm, int maxAllowedDistance)
        {
            double normalizedDistance = GetNormalizedDistance(input, otherWord, distanceAlgorithm, maxAllowedDistance);
            if (normalizedDistance == -1)
                return -1;

            if (!this.guid.Equals(Guid.Empty))
            {
                double lastUsedResult = GetNormalizedLastUsedMetric(otherWord, 14);
                double mostUsedResult = GetNormalizedMostUsedMetric(otherWord, 14);
                double final = (0.8 * normalizedDistance + 0.05 * lastUsedResult + 0.15 * mostUsedResult);
                return final;
            }
            else
                return normalizedDistance; 
        }
    }
}
