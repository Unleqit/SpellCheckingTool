using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool
{
    public class WordStatistic
    {
        public string Word { get; private set; } = "";
        public int UsageCount { get; private set; }
        public DateTime LastUsedAt { get; private set; }

        private WordStatistic() { }

        public WordStatistic(string word)
        {
            Word = word;
            UsageCount = 0;
            LastUsedAt = DateTime.UtcNow;
        }

        public void Increment()
        {
            UsageCount++;
            LastUsedAt = DateTime.UtcNow;
        }
    }
}
