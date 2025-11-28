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
        private readonly UserService _service;

        public ClientAuthService(UserService service)
        {
            _service = service;
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

            var result = isRegister
                ? _service.Register(username, hashed)
                : _service.Login(username, hashed);

            if (!result.Success)
            {
                Console.WriteLine($"{(isRegister ? "Registration" : "Login")} failed: {result.ErrorMessage}");
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