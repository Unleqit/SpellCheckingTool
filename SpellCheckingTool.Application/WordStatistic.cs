using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application;
    public class WordStatistic
    {
        public Word Word { get; private set; }   
        public int UsageCount { get; private set; }
        public DateTime LastUsedAt { get; private set; }

        private WordStatistic() { }

        public WordStatistic(Word word)
        {
            Word = word;
            UsageCount = 0;
            LastUsedAt = DateTime.UtcNow;
        }
        public WordStatistic(Word word, int usageCount, DateTime lastUsedAt)
        {
            Word = word;
            UsageCount = usageCount;
            LastUsedAt = lastUsedAt;
        }

        public void Increment()
        {
            UsageCount++;
            LastUsedAt = DateTime.UtcNow;
        }
    }
