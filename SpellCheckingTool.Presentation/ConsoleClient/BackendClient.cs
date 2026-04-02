using Newtonsoft.Json;
using SpellCheckingTool.Presentation.ConsoleClient.Exceptions;
using System.Net;
using System.Text;

namespace SpellCheckingTool.Presentation.ConsoleClient
{
    public class BackendClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _backendUrl;

        public BackendClient(HttpClient httpClient, string backendUrl)
        {
            _httpClient = httpClient;
            _backendUrl = backendUrl.TrimEnd('/');
        }

        public async Task<ApiResult<T>> PostAsync<T>(string url, object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync($"{_backendUrl}{url}", content);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResult<T>
                    {
                        IsSuccess = false,
                        StatusCode = response.StatusCode,
                        ErrorMessage = ExtractErrorMessage(body, response.StatusCode)
                    };
                }

                try
                {
                    var data = JsonConvert.DeserializeObject<T>(body);

                    return new ApiResult<T>
                    {
                        IsSuccess = true,
                        Data = data,
                        StatusCode = response.StatusCode
                    };
                }
                catch (JsonException ex)
                {
                    throw new BackendResponseParseException(typeof(T).Name, ex);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new BackendConnectionException(
                    $"Could not connect to backend at '{url}'.", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new BackendConnectionException(
                    $"Request to '{url}' timed out.", ex);
            }
        }
        private string ExtractErrorMessage(string body, HttpStatusCode status)
        {
            try
            {
                var errorJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);

                if (errorJson != null && errorJson.TryGetValue("error", out var message))
                {
                    return $"{message}";
                }
            }
            catch
            {
                // ignore parsing error
            }

            return $"{status}";
        }
    }
}
