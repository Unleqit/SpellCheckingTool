using SpellCheckingTool.Domain.WordStats;

namespace SpellCheckingTool.Application.Users;
    public interface IUserWordStatsRepository
    {
        void IncrementWord(Guid userId, string word);
        IReadOnlyCollection<WordStatistic> GetWordStats(Guid userId);
    }
