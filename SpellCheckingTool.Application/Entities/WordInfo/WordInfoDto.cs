using SpellCheckingTool.Application.UserWordStats;

namespace SpellCheckingTool.Application.Entities.WordInfo
{
    public class WordInfoDto
    {
        public string Key { get; set;  }
        public WordStatisticDto Statistic { get; set; } = null!;
    }
}
