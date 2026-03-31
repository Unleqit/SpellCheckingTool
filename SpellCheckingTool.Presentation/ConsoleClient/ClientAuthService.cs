using Newtonsoft.Json;
using SpellCheckingTool.Application.LoginResponse;
using SpellCheckingTool.Application.UserStatsResponse;
using SpellCheckingTool.Application.UserWordsFileResponse;
using SpellCheckingTool.Domain.WordStats;
using SpellCheckingTool.Domain.WordTree;
using System.Text;

namespace SpellCheckingTool.Presentation.ConsoleClient;
public class ClientAuthService
    {
    private readonly BackendClient _client;

    public ClientAuthService(string backendUrl)
    {
        var httpClient = new HttpClient();
        _client = new BackendClient(httpClient, backendUrl);
    }

    public async Task<AuthSession?> RunAuthenticationFlow()
    {
        Console.Write("Username (add '--register' to register): ");
        string input = Console.ReadLine()?.Trim() ?? "";

        bool isRegister = input.Contains("--register");
        string username = input.Replace("--register", "").Trim();

        return await HandleAuth(username, isRegister);
    }

    private async Task<bool> ExecuteWordAction(string url, Guid userId, string word, string errorMessage)
    {
        var result = await _client.PostAsync<SuccessResponse>(
            url,
            new { userId, word },
            errorMessage);

        if (!result.IsSuccess)
        {
            Console.WriteLine(result.ErrorMessage);
            return false;
        }

        return result.Data?.Success ?? false;
    }

    public async Task<bool> AddWord(Guid userId, string word) => await
        ExecuteWordAction("/api/v1/users/words/add", userId, word, "Could not save word");

    public async Task<bool> DeleteWord(Guid userId, string word) => await
        ExecuteWordAction("/api/v1/users/words/delete", userId, word, "Could not delete word");

    public async Task<bool> TrackWordUsage(Guid userId, string word) => await
        ExecuteWordAction("/api/v1/users/words/track", userId, word, "Could not track word");


    private async Task<AuthSession?> HandleAuth(string username, bool isRegister)
    {
        Console.Write(isRegister ? "Choose a password: " : "Password: ");
        string password = ReadPassword();

        string endpoint = isRegister ? "register" : "login";
        string action = isRegister ? "Registration" : "Login";

        var result = await _client.PostAsync<LoginResponseDto>(
            $"/api/v1/users/{endpoint}",
            new { username, password },
            $"{action} failed");

        if (!result.IsSuccess)
        {
            Console.WriteLine(result.ErrorMessage);
            return null;
        }

        Console.WriteLine($"{action} successful!");

        var dto = result.Data;

        if (dto == null || dto.UserId == Guid.Empty)
        {
            Console.WriteLine("Warning: authenticated, but invalid response.");
            return null;
        }

        var domain = LoginResponseMapper.ToDomain(dto);

        return new AuthSession
        {
            UserId = domain.UserId,
            Username = domain.Username,
            IsAuthenticated = true
        };
    }

    private string ReadPassword()
        {
            var password = new StringBuilder();
            ConsoleKeyInfo key;

            while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b");
                }
                else
                {
                    password.Append(key.KeyChar);
                    Console.Write("*");
                }
            }

            Console.WriteLine();
            return password.ToString();
        }

    public async Task<IReadOnlyList<Word>> GetWords(Guid userId)
    {
        var result = await _client.PostAsync<UserWordsFileResponseDto>(
            "/api/v1/users/words/file",
            new { userId },
            "Could not load words");

        if (!result.IsSuccess)
        {
            Console.WriteLine(result.ErrorMessage);
            return Array.Empty<Word>();
        }

        var dto = result.Data;

        if (dto?.Words == null)
            return Array.Empty<Word>();

        return dto.Words
            .Select(w => WordMapper.ToDomain(w))
            .ToList();
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