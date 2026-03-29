using System.Security.Cryptography;
using System.Text;

namespace SpellCheckingTool.Application.Authentication
{
    public class HashService
    {
        public static string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
