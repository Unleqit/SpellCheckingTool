using Newtonsoft.Json;
using SpellCheckingTool.Presentation.ConsoleClient.Exceptions;
using System.Text;

namespace SpellCheckingTool.Presentation.ConsoleClient;
public class ClientAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _backendUrl;

        public ClientAuthService(string backendUrl)
        {
            _backendUrl = backendUrl.TrimEnd('/');
            _httpClient = new HttpClient();
        }

        public AuthSession? RunAuthenticationFlow()
        {
            Console.Write("Username (add '--register' to register): ");
            string input = Console.ReadLine()?.Trim() ?? "";

            bool isRegister = input.Contains("--register");
            string username = input.Replace("--register", "").Trim();

            return HandleAuth(username, isRegister);
        }

    public bool AddWord(Guid userId, string word)
    {
        var payload = new
        {
            userId = userId,
            word = word
        };

        string requestJson = JsonConvert.SerializeObject(payload);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        string url = $"{_backendUrl}/api/v1/users/words/add";

        try
        {
            var response = _httpClient.PostAsync(url, content).Result;
            string responseBody = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errorJson =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);

                    if (errorJson != null && errorJson.TryGetValue("error", out var message))
                    {
                        Console.WriteLine($"Could not save word: {message}");
                    }
                    else
                    {
                        Console.WriteLine($"Could not save word: {response.StatusCode}");
                    }
                }
                catch
                {
                    Console.WriteLine($"Could not save word: {response.StatusCode}");
                }

                return false;
            }

            try
            {
                var json = JsonConvert.DeserializeObject<SuccessResponseDto>(responseBody);
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
            Console.WriteLine($"Error saving word: {ex.Message}");
            return false;
        }
    }

    public bool DeleteWord(Guid userId, string word)
    {
        return Post("/api/v1/users/words/delete", new
        {
            userId,
            word
        });
    }

    public bool TrackWordUsage(Guid userId, string word)
    {
        return Post("/api/v1/users/words/track", new
        {
            userId,
            word
        });
    }

    private AuthSession? HandleAuth(string username, bool isRegister)
    {
        Console.Write(isRegister ? "Choose a password: " : "Password: ");
        string password = ReadPassword();

        string requestJson = JsonConvert.SerializeObject(new { username = username, password = password });
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        string endpoint = isRegister ? "register" : "login";
        string url = $"{_backendUrl}/api/v1/users/{endpoint}";

        HttpResponseMessage response;
        try
        {
            response = _httpClient.PostAsync(url, content).Result;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(new BackendConnectionException(
                $"Could not connect to backend at '{url}'.", ex).Message);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine(new BackendConnectionException(
                $"The request to '{url}' timed out.", ex).Message);
            return null;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(new BackendConnectionException(
                $"The HTTP request to '{url}' could not be started.", ex).Message);
            return null;
        }

        string responseBody = response.Content.ReadAsStringAsync().Result;

        if (!response.IsSuccessStatusCode)
        {
            string action = isRegister ? "Registration" : "Login";

            try
            {
                Dictionary<string, string>? json =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);

                if (json != null && json.TryGetValue("error", out var message))
                {
                    Console.WriteLine($"{action} failed: {message}");
                }
                else
                {
                    Console.WriteLine($"{action} failed: {response.StatusCode}");
                }
            }
            catch
            {
                Console.WriteLine($"{action} failed: {response.StatusCode}");
            }

            return null;
        }

        Console.WriteLine($"{(isRegister ? "Registration" : "Login")} successful!");

        try
        {
            var json = JsonConvert.DeserializeObject<LoginResponseDto>(responseBody);

            if (json != null && json.UserId != Guid.Empty)
            {
                return new AuthSession
                {
                    UserId = json.UserId,
                    Username = json.Username,
                    IsAuthenticated = true
                };
            }
        }
        catch
        {
        }

        Console.WriteLine("Warning: authenticated, but no usable session payload was returned.");
        return null;
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

       

    public IReadOnlyList<UserDictionaryWordDto> GetWords(Guid userId)
    {
        var payload = new { userId };
        string requestJson = JsonConvert.SerializeObject(payload);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        string url = $"{_backendUrl}/api/v1/users/words/file";

        try
        {
            var response = _httpClient.PostAsync(url, content).Result;
            string responseBody = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Could not load words: {response.StatusCode}");
                return Array.Empty<UserDictionaryWordDto>();
            }

            var json = JsonConvert.DeserializeObject<UserWordsFileResponseDto>(responseBody);
            return json?.Words?.ToList() ?? new List<UserDictionaryWordDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading words: {ex.Message}");
            return Array.Empty<UserDictionaryWordDto>();
        }
    }

    public IReadOnlyList<UserWordStatDto> GetStats(Guid userId)
    {
        var payload = new { userId };
        string requestJson = JsonConvert.SerializeObject(payload);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        string url = $"{_backendUrl}/api/v1/users/words/stats";

        try
        {
            var response = _httpClient.PostAsync(url, content).Result;
            string responseBody = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Could not load stats: {response.StatusCode}");
                return Array.Empty<UserWordStatDto>();
            }

            var json = JsonConvert.DeserializeObject<UserStatsResponseDto>(responseBody);
            return json?.Stats?.ToList() ?? new List<UserWordStatDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading stats: {ex.Message}");
            return Array.Empty<UserWordStatDto>();
        }
    }
    private bool Post(string relativeUrl, object payload)
    {
        string requestJson = JsonConvert.SerializeObject(payload);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
        string url = $"{_backendUrl}{relativeUrl}";

        try
        {
            var response = _httpClient.PostAsync(url, content).Result;
            string responseBody = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Request failed: {response.StatusCode}");
                return false;
            }

            try
            {
                var json = JsonConvert.DeserializeObject<SuccessResponseDto>(responseBody);
                return json?.Success ?? false;
            }
            catch (JsonException ex)
            {
                Console.WriteLine(new BackendResponseParseException("success response", ex).Message);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(new BackendConnectionException(
                $"Request to '{url}' failed.", ex).Message);
            return false;
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine(new BackendConnectionException(
                $"Request to '{url}' timed out.", ex).Message);
            return false;
        }
    }
}