using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool
{
    public class WordStatistic
    {
        public string Word { get; set; } = "";
        public int UsageCount { get; set; }
        public DateTime LastUsedAt { get; set; }

        public WordStatistic() { }

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
