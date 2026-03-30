using Newtonsoft.Json;
using SpellCheckingTool.Application.LoginResponse;
using SpellCheckingTool.Application.UserStatsResponse;
using SpellCheckingTool.Application.UserWordsFileResponse;
using SpellCheckingTool.Domain.WordStats;
using SpellCheckingTool.Domain.WordTree;
using SpellCheckingTool.Presentation.ConsoleClient.Exceptions;
using System.Net;
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

    public async Task<bool> AddWord(Guid userId, string word)
    {
        try
        {
            var (success, body, status) = await _client.PostAsync(
                "/api/v1/users/words/add",
                new { userId, word });

            if (!success)
            {
                TryPrintError(body, status, "Could not save word");
                return false;
            }

            try
            {
                var json = JsonConvert.DeserializeObject<SuccessResponse>(body);
                return json?.Success ?? false;
            }
            catch (JsonException ex)
            {
                Console.WriteLine(new BackendResponseParseException("success response", ex).Message);
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    private void TryPrintError(string responseBody, HttpStatusCode status, string prefix)
    {
        try
        {
            var errorJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);

            if (errorJson != null && errorJson.TryGetValue("error", out var message))
            {
                Console.WriteLine($"{prefix}: {message}");
            }
            else
            {
                Console.WriteLine($"{prefix}: {status}");
            }
        }
        catch
        {
            Console.WriteLine($"{prefix}: {status}");
        }
    }

    public async Task<bool> DeleteWord(Guid userId, string word)
    {
        try
        {
            var (success, body, status) = await _client.PostAsync(
                "/api/v1/users/words/delete",
                new { userId, word });

            if (!success)
            {
                TryPrintError(body, status, "Could not delete word");
                return false;
            }

            var json = JsonConvert.DeserializeObject<SuccessResponse>(body);
            return json?.Success ?? false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public async Task<bool> TrackWordUsage(Guid userId, string word)
    {
        try
        {
            var (success, body, status) = await _client.PostAsync(
                "/api/v1/users/words/track",
                new { userId, word });

            if (!success)
            {
                TryPrintError(body, status, "Could not track word");
                return false;
            }

            var json = JsonConvert.DeserializeObject<SuccessResponse>(body);
            return json?.Success ?? false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    private async Task<AuthSession?> HandleAuth(string username, bool isRegister)
    {
        Console.Write(isRegister ? "Choose a password: " : "Password: ");
        string password = ReadPassword();

        string endpoint = isRegister ? "register" : "login";

        try
        {
            var (success, body, status) = await _client.PostAsync(
                $"/api/v1/users/{endpoint}",
                new { username, password });

            if (!success)
            {
                string action = isRegister ? "Registration" : "Login";
                TryPrintError(body, status, $"{action} failed");
                return null;
            }

            Console.WriteLine($"{(isRegister ? "Registration" : "Login")} successful!");

            try
            {
                var dto = JsonConvert.DeserializeObject<LoginResponseDto>(body);
                if (dto == null || dto.UserId == Guid.Empty)
                {
                    Console.WriteLine($"DTO Deserialization failed: {dto}");
                }

                var domain = LoginResponseMapper.ToDomain(dto);


                return new AuthSession
                {
                    UserId = domain.UserId,
                    Username = domain.Username,
                    IsAuthenticated = true
                };
            }
            catch
            {
            }

            Console.WriteLine("Warning: authenticated, but no usable session payload was returned.");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
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
        try
        {
            var (success, body, status) = await _client.PostAsync(
                "/api/v1/users/words/file",
                new { userId });

            if (!success)
            {
                Console.WriteLine($"Could not load words: {status}");
                return Array.Empty<Word>();
            }

            var json = JsonConvert.DeserializeObject<UserWordsFileResponseDto>(body);

            if (json?.Words == null)
                return Array.Empty<Word>();

            return json.Words
                .Select(w => WordMapper.ToDomain(w))
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading words: {ex.Message}");
            return Array.Empty<Word>();
        }
    }

    public async Task<IReadOnlyList<WordStatistic>> GetStats(Guid userId)
    {
        try
        {
            var (success, body, status) = await _client.PostAsync(
                "/api/v1/users/words/stats",
                new { userId });

            if (!success)
            {
                Console.WriteLine($"Could not load stats: {status}");
                return Array.Empty<WordStatistic>();
            }

            var dto = JsonConvert.DeserializeObject<UserStatsResponseDto>(body);

            if (dto?.Stats == null)
                return Array.Empty<WordStatistic>();

            var domain = UserStatsResponseMapper.ToDomain(dto);
            return domain.Stats;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading stats: {ex.Message}");
            return Array.Empty<WordStatistic>();
        }
    }
}