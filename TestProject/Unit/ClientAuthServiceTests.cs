using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpellCheckingTool.Presentation.ConsoleClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.Unit;

    [TestClass]
    public class ClientAuthServiceTests
    {
        [TestMethod]
        public void Hash_ReturnsExpectedSHA256Hash()
        {
            string password = "MySecurePassword123";

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            string expected = Convert.ToHexString(hash);

            string actual = ClientAuthService.Hash(password);

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

            string actual = ClientAuthService.Hash(password);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Hash_SameInput_ReturnsSameHash()
        {
            string password = "abc123";

            string hash1 = ClientAuthService.Hash(password);
            string hash2 = ClientAuthService.Hash(password);

            Assert.AreEqual(hash1, hash2);
        }

        [TestMethod]
        public void Hash_DifferentInputs_ReturnDifferentHashes()
        {
            string password1 = "abc123";
            string password2 = "abc124";

            string hash1 = ClientAuthService.Hash(password1);
            string hash2 = ClientAuthService.Hash(password2);

            Assert.AreNotEqual(hash1, hash2);
        }
    }