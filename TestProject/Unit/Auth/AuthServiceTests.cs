using SpellCheckingTool.Application.Authentication;
using SpellCheckingTool.Domain.Users;
using TestProject.Fakes;

namespace TestProject.Unit.Auth
{
    [TestClass]
    public class AuthServiceTests
    {
        private FakeUserRepository _userRepository = null!;
        private FakeUserSettingsRepository _settingsRepository = null!;

        private AuthService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _userRepository = new FakeUserRepository();
            _settingsRepository = new FakeUserSettingsRepository();
            _service = new AuthService(_userRepository, _settingsRepository);
        }

        [TestMethod]
        public void Register_ShouldCreateUserSuccessfully()
        {
            var result = _service.Register("armin", "HASH");

            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual("armin", result.Value!.Username);
            Assert.AreEqual(HashService.Hash("HASH"), result.Value.PasswordHash);
            Assert.IsNotNull(_userRepository.GetByUsername("armin"));
        }

        [TestMethod]
        public void Register_ShouldTrimUsernameBeforeSaving()
        {
            var result = _service.Register("  armin  ", "HASH");

            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual("armin", result.Value!.Username);

            var loaded = _userRepository.GetByUsername("armin");
            Assert.IsNotNull(loaded);
        }

        [TestMethod]
        public void Register_WithEmptyUsername_ShouldFail()
        {
            var result = _service.Register("", "HASH");

            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.AreEqual("Username is required.", result.ErrorMessage);
        }

        [TestMethod]
        public void Register_WithWhitespaceUsername_ShouldFail()
        {
            var result = _service.Register("   ", "HASH");

            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.AreEqual("Username is required.", result.ErrorMessage);
        }

        [TestMethod]
        public void Register_WithEmptyPassword_ShouldFail()
        {
            var result = _service.Register("armin", "");

            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.AreEqual("Password is required.", result.ErrorMessage);
        }

        [TestMethod]
        public void Register_WithWhitespacePassword_ShouldFail()
        {
            var result = _service.Register("armin", "   ");

            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.AreEqual("Password is required.", result.ErrorMessage);
        }

        [TestMethod]
        public void Register_WithDuplicateUsername_ShouldFail()
        {
            _userRepository.Seed(new User(Guid.NewGuid(), "armin", HashService.Hash("HASH1"), DateTime.UtcNow));

            var result = _service.Register("Armin", "HASH2");

            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.AreEqual("Username already exists.", result.ErrorMessage);
        }

        [TestMethod]
        public void Register_ShouldInitializeDefaultSettings()
        {
            var result = _service.Register("armin", "HASH");

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, _settingsRepository.SetSettingsCallCount);
            Assert.AreEqual("armin", _settingsRepository.LastSetUsername);
            Assert.IsNotNull(_settingsRepository.LastSetSettings);
        }

        [TestMethod]
        public void Login_WithCorrectCredentials_ShouldReturnUser()
        {
            var user = new User(Guid.NewGuid(), "armin", HashService.Hash("HASH"), DateTime.UtcNow);
            _userRepository.Seed(user);

            var result = _service.Login("armin", "HASH");

            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(user.Id, result.Value!.Id);
        }

        [TestMethod]
        public void Login_WithWrongPassword_ShouldFail()
        {
            var user = new User(Guid.NewGuid(), "armin", HashService.Hash("HASH"), DateTime.UtcNow);
            _userRepository.Seed(user);

            var result = _service.Login("armin", "WRONG");

            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.AreEqual("Invalid password.", result.ErrorMessage);
        }

        [TestMethod]
        public void Login_WithUnknownUsername_ShouldFail()
        {
            var result = _service.Login("missing", "HASH");

            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.AreEqual("User not found.", result.ErrorMessage);
        }
    }
}
