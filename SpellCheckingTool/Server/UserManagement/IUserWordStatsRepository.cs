using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool
{
    public interface IUserWordStatsRepository
    {
        void IncrementWord(Guid userId, string word);
        IReadOnlyCollection<WordStatistic> GetWordStats(Guid userId);
    }
}
