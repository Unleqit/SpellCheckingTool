using SpellCheckingTool.Application.UserStatsResponse;
using SpellCheckingTool.Domain.WordStats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                new { userId },
                "Could not load stats");

            if (!result.IsSuccess)
            {
                Console.WriteLine(result.ErrorMessage);
                return Array.Empty<WordStatistic>();
            }

            return result.Data?.Stats != null
                ? UserStatsResponseMapper.ToDomain(result.Data).Stats
                : Array.Empty<WordStatistic>();
        }
    }
}
