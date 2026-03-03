namespace SpellCheckingTool.Domain.WordStats;
    public class WordInfo
    {
        public string Key { get; }
        public WordStatistic Statistic { get; }

        public WordInfo(string key, WordStatistic statistic)
        {
            Key = key;
            Statistic = statistic;
        }
    }
