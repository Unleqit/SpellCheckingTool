namespace SpellCheckingTool.Infrastructure.UserPersistence;
    internal class WordStatisticStorage
    {
        public string Word { get; set; } = "";
        public int UsageCount { get; set; }
        public DateTime LastUsedAt { get; set; }
    }