using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;


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

            var json = JsonConvert.DeserializeObject<SuccessResponseDto>(responseBody);
            return json?.Success == true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving word: {ex.Message}");
            return false;
        }
    }

    private AuthSession? HandleAuth(string username, bool isRegister)
    {
        Console.Write(isRegister ? "Choose a password: " : "Password: ");
        string password = ReadPassword();
        string hashed = Hash(password);
        var payload = new
        {
            username = username,
            hashedPassword = hashed
        };

        string requestJson = JsonConvert.SerializeObject(payload);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        string url = isRegister
            ? $"{_backendUrl}/api/v1/users/register"
            : $"{_backendUrl}/api/v1/users/login";

        HttpResponseMessage response;
        try
        {
            response = _httpClient.PostAsync(url, content).Result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to backend: {ex.Message}");
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

        public static string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

    public IReadOnlyList<UserWordDto> GetWords(Guid userId)
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
                return Array.Empty<UserWordDto>();
            }

            var json = JsonConvert.DeserializeObject<UserWordsFileResponseDto>(responseBody);
            return json?.Words?.ToList() ?? new List<UserWordDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading words: {ex.Message}");
            return Array.Empty<UserWordDto>();
        }
    }

    public IReadOnlyList<UserWordDto> GetStats(Guid userId)
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
                return Array.Empty<UserWordDto>();
            }

            var json = JsonConvert.DeserializeObject<UserStatsResponseDto>(responseBody);
            return json?.Stats?.ToList() ?? new List<UserWordDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading stats: {ex.Message}");
            return Array.Empty<UserWordDto>();
        }
    }
}