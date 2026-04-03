using SpellCheckingTool.Application.UserStatsResponse;
using SpellCheckingTool.Domain.WordStats;

namespace SpellCheckingTool.Presentation.ConsoleClient.ClientServices
{
    public class ClientStatsService
    {
        private readonly BackendClient _client;

        public ClientStatsService(BackendClient client)
        {
            _client = client;
        }

        public async Task<IReadOnlyList<WordStatistic>> GetStats(Guid userId)
        {
            var result = await _client.PostAsync<UserStatsResponseDto>(
                "/api/v1/users/words/stats",
                new { userId });

            if (!result.IsSuccess)
            {
                Console.WriteLine($"Could not load stats: {result.ErrorMessage}");
                return Array.Empty<WordStatistic>();
            }

            return result.Data?.Stats != null
                ? UserStatsResponseMapper.ToDomain(result.Data).Stats
                : Array.Empty<WordStatistic>();
        }
    }
}
