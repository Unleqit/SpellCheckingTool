using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain;
using SpellCheckingTool.Domain.Suggestion;
using SpellCheckingTool.Domain.WordStats;

namespace SpellCheckingTool.Application.Suggestion.SuggestionService
{
    public class StatisticSuggestionService: SuggestionService
    {
        private const double distanceWeight = 0.8;
        private const double lastUsedWeight = 0.05;
        private const double frequencyWeight = 0.15;

        private UserService userService;
        private IEnumerable<WordStatistic> currentStats = [];
        private Guid guid;
        private int lastNDays;

        public StatisticSuggestionService(IWordStorage tree, IDistanceAlgorithm distanceAlgorithm, 
            UserService userService, Guid guid, int lastNDays = 14): base(tree, distanceAlgorithm)
        {
            this.userService = userService;
            this.guid = guid;
            this.lastNDays = lastNDays;
        }

        /// <summary>
        /// Fetches the current user statistics and caches them for the duration of this suggestion retrieval operation
        /// </summary>
        protected override void OnPreWalk()
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
        private double GetNormalizedFrequencyMetric(Word input, int lastNDays = 14)
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
        /// Returns a value between 0 and 1 representing a combination of the weighted distance of two words determined by the distance algorithm used to instantiate this class, 
        /// while also factoring in if or when the given input word was last used and how often it was used overall.
        /// Returns -1, if their distance exceeds the provided threshold.
        /// </summary>
        protected override double ComputeScore(Word inputWord, Word otherWord, int maxAllowedDistance)
        {
            double distance = base.ComputeScore(inputWord, otherWord, maxAllowedDistance);
            if (distance == -1)
                return -1;

            if (!this.guid.Equals(Guid.Empty))
            {
                double lastUsedResult = GetNormalizedLastUsedMetric(otherWord, 14);
                double frequencyResult = GetNormalizedFrequencyMetric(otherWord, 14);
                double final = (distanceWeight * distance + lastUsedWeight * lastUsedResult + frequencyWeight * frequencyResult);

                return final;
            }
            else
                return distance; 
        }
    }
}
