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

        public async Task<(bool Success, string Body, HttpStatusCode Status)> PostAsync(string url, object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync($"{_backendUrl}{url}", content);
                var body = await response.Content.ReadAsStringAsync();

                return (response.IsSuccessStatusCode, body, response.StatusCode);
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
    }
}
