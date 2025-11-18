using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool
{
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
}
