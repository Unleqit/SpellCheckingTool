using SpellCheckingTool.Application.LoginResponse;
using System.Text;

namespace SpellCheckingTool.Presentation.ConsoleClient.ClientServices
{
    public class ClientAuthService
    {
        private readonly BackendClient _client;

        public ClientAuthService(BackendClient client)
        {
            _client = client;
        }

        public async Task<AuthSession?> RunAuthenticationFlow()
        {
            var (username, isRegister) = ReadUsername();
            var password = ReadPassword(isRegister);

            var session = await Authenticate(username, password, isRegister);

            if (session == null)
            {
                Console.WriteLine("Authentication failed.");
                return null;
            }

            string action = isRegister ? "Registration" : "Login";
            Console.WriteLine($"{action} successful!");
            return session;
        }

        public async Task<AuthSession?> Authenticate(string username, string password, bool isRegister)
        {
            string endpoint = isRegister ? "register" : "login";
            string action = isRegister ? "Registration" : "Login";

            var result = await _client.PostAsync<LoginResponseDto>(
                $"/api/v1/users/{endpoint}",
                new { username, password });

            if (!result.IsSuccess)
            {
                Console.WriteLine($"{action} failed: {result.ErrorMessage}");
                return null;
            }

            var dto = result.Data;

            if (dto == null || dto.UserId == Guid.Empty)
                return null;

            var domain = LoginResponseMapper.ToDomain(dto);

            return new AuthSession
            {
                UserId = domain.UserId,
                Username = domain.Username,
                IsAuthenticated = true
            };
        }

        private (string Username, bool IsRegister) ReadUsername()
        {
            Console.Write("Username (add '--register' to register): ");
            string input = Console.ReadLine()?.Trim() ?? "";

            bool isRegister = input.Contains("--register");
            string username = input.Replace("--register", "").Trim();

            return (username, isRegister);
        }

        private string ReadPassword(bool isRegister)
        {
            Console.Write(isRegister ? "Choose a password: " : "Password: ");

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
    }
}
