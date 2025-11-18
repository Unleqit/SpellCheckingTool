using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool
{
    internal class WordStatisticStorage
    {
        public string Word { get; set; } = "";
        public int UsageCount { get; set; }
        public DateTime LastUsedAt { get; set; }
    }
}
