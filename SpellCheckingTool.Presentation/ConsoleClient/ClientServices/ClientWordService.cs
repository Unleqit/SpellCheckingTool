using SpellCheckingTool.Application.UserWordsFileResponse;
using SpellCheckingTool.Domain.WordTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool.Presentation.ConsoleClient.ClientServices
{
    public class ClientWordService
    {
        private readonly BackendClient _client;

        public ClientWordService(BackendClient client)
        {
            _client = client;
        }

        private async Task<bool> ExecuteWordAction(string url, Guid userId, string word, string errorMessage)
        {
            var result = await _client.PostAsync<SuccessResponse>(
                url,
                new { userId, word });

            if (!result.IsSuccess)
            {
                Console.WriteLine($"{errorMessage}: {result.ErrorMessage}");
                return false;
            }

            return result.Data?.Success ?? false;
        }

        public Task<bool> AddWord(Guid userId, string word) =>
            ExecuteWordAction("/api/v1/users/words/add", userId, word, "Could not save word");

        public Task<bool> DeleteWord(Guid userId, string word) =>
            ExecuteWordAction("/api/v1/users/words/delete", userId, word, "Could not delete word");

        public Task<bool> TrackWordUsage(Guid userId, string word) =>
            ExecuteWordAction("/api/v1/users/words/track", userId, word, "Could not track word");

        public async Task<IReadOnlyList<Word>> GetWords(Guid userId)
        {
            var result = await _client.PostAsync<UserWordsFileResponseDto>(
                "/api/v1/users/words/file",
                new { userId });

            if (!result.IsSuccess)
            {
                Console.WriteLine($"Could not load words: {result.ErrorMessage}");
                return Array.Empty<Word>();
            }

            var dto = result.Data;

            if (dto?.Words == null)
                return Array.Empty<Word>();

            return dto.Words
                .Select(w => WordMapper.ToDomain(w))
                .ToList();
        }
    }
}
