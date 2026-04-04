using SpellCheckingTool.Application.Common;
using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Users;

namespace SpellCheckingTool.Application.Authentication
{
    public class AuthService
    {
        IUserRepository _userRepository;
        IUserSettingsRepository _settingsRepository;

        public AuthService(IUserRepository userRepository, IUserSettingsRepository settingsRepository)
        {
            this._userRepository = userRepository;
            this._settingsRepository = settingsRepository;
        }

        public OperationResult<User> Register(string username, string password)
        {
            username = username?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(username))
                return OperationResult<User>.Fail("Username is required.");

            if (string.IsNullOrWhiteSpace(password))
                return OperationResult<User>.Fail("Password is required.");

            if (_userRepository.GetByUsername(username) != null)
                return OperationResult<User>.Fail("Username already exists.");

            string hashedPassword = HashService.Hash(password);

            var user = new User(
                Guid.NewGuid(),
                username.Trim(),
                hashedPassword,
                DateTime.UtcNow
                );

            _userRepository.Add(user);
            _settingsRepository.SetSettings(user.Username, UserSettings.Default);

            return OperationResult<User>.Ok(user);
        }

        public OperationResult<User> Login(string username, string password)
        {
            username = username?.Trim() ?? "";

            var user = _userRepository.GetByUsername(username);
            if (user == null)
                return OperationResult<User>.Fail("User not found.");

            string hashedPassword = HashService.Hash(password);

            if (!string.Equals(user.PasswordHash, hashedPassword, StringComparison.Ordinal))
                return OperationResult<User>.Fail("Invalid password.");

            return OperationResult<User>.Ok(user);
        }

        public OperationResult<bool> UsernameExists(string username)
        {
            username = username?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(username))
                return OperationResult<bool>.Fail("Username is required.");

            bool exists = _userRepository.GetByUsername(username.Trim()) != null;
            return OperationResult<bool>.Ok(exists);
        }
    }
}
