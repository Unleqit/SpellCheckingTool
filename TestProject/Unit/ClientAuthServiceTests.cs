using SpellCheckingTool.Application.Authentication;
using System.Security.Cryptography;
using System.Text;

namespace TestProject.Unit;

    [TestClass]
    public class HashServiceTests
    {
        [TestMethod]
        public void Hash_ReturnsExpectedSHA256Hash()
        {
            string password = "MySecurePassword123";

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            string expected = Convert.ToHexString(hash);

            string actual = HashService.Hash(password);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Hash_EmptyString_ReturnsCorrectHash()
        {
            string password = "";

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            string expected = Convert.ToHexString(hash);

            string actual = HashService.Hash(password);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Hash_SameInput_ReturnsSameHash()
        {
            string password = "abc123";

            string hash1 = HashService.Hash(password);
            string hash2 = HashService.Hash(password);

            Assert.AreEqual(hash1, hash2);
        }

        [TestMethod]
        public void Hash_DifferentInputs_ReturnDifferentHashes()
        {
            string password1 = "abc123";
            string password2 = "abc124";

            string hash1 = HashService.Hash(password1);
            string hash2 = HashService.Hash(password2);

            Assert.AreNotEqual(hash1, hash2);
        }
    }