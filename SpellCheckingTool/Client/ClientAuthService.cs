using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace SpellCheckingTool.Client
{
    public class ClientAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _backendUrl;

        public ClientAuthService(string backendUrl)
        {
            _backendUrl = backendUrl.TrimEnd('/');
            _httpClient = new HttpClient();
        }

        public void RunAuthenticationFlow()
        {
            Console.Write("Username (add '--register' to register): ");
            string input = Console.ReadLine()?.Trim() ?? "";

            bool isRegister = input.Contains("--register");
            string username = input.Replace("--register", "").Trim();

            HandleAuth(username, isRegister);
        }

        private void HandleAuth(string username, bool isRegister)
        {
            Console.Write(isRegister ? "Choose a password: " : "Password: ");
            string password = ReadPassword();
            string hashed = Hash(password);

            // Payload muss param-Namen im Controller matchen:
            var payload = new
            {
                username = username,
                hashedPassword = hashed
            };

            string json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

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
                return;
            }

            string responseBody = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"{(isRegister ? "Registration" : "Login")} failed: {response.StatusCode}");
                Console.WriteLine(responseBody); //TO DO: Parse the error properly in the frontend
                return;
            }

            Console.WriteLine($"{(isRegister ? "Registration" : "Login")} successful!");
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
    }
}