using SpellCheckingTool.Application.Common;
using SpellCheckingTool.Domain.Users;
using SpellCheckingTool.Domain.WordStats;

namespace SpellCheckingTool.Application.Users;

public class UserService
{
    private readonly IUserRepository _userRepo;
    private readonly IUserWordStatsRepository _wordRepo;

    public UserService(IUserRepository userRepo, IUserWordStatsRepository wordRepo)
    {
        _userRepo = userRepo;
        _wordRepo = wordRepo;
    }

    public OperationResult<User> Register(string username, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(username))
            return OperationResult<User>.Fail("Username is required.");
        if (string.IsNullOrWhiteSpace(hashedPassword))
            return OperationResult<User>.Fail("Password is required.");
        if (_userRepo.GetByUsername(username) != null)
            return OperationResult<User>.Fail("Username already exists.");

        var user = new User(
            Guid.NewGuid(),
            username.Trim(),
            hashedPassword,
            DateTime.UtcNow
        );

        _userRepo.Add(user);
        return OperationResult<User>.Ok(user);
    }

    public OperationResult<User> Login(string username, string hashedPassword)
    {
        var user = _userRepo.GetByUsername(username);
        if (user == null)
            return OperationResult<User>.Fail("User not found.");

        if (!string.Equals(user.PasswordHash, hashedPassword, StringComparison.Ordinal))
            return OperationResult<User>.Fail("Invalid password.");

        return OperationResult<User>.Ok(user);
    }

    public OperationResult<bool> AddWord(Guid userId, string word)
    {
        if (_userRepo.GetById(userId) == null)
            return OperationResult<bool>.Fail("User not found.");

        _wordRepo.IncrementWord(userId, word);
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<IReadOnlyCollection<WordStatistic>> GetStats(Guid userId)
    {
        if (_userRepo.GetById(userId) == null)
            return OperationResult<IReadOnlyCollection<WordStatistic>>.Fail("User not found.");

        var stats = _wordRepo.GetWordStats(userId);
        return OperationResult<IReadOnlyCollection<WordStatistic>>.Ok(stats);
    }

    /// <summary>
    /// Returns the raw (unsorted) word stats for a user.
    /// This replaces Presentation reaching into an Infrastructure store directly.
    /// </summary>
    public OperationResult<IReadOnlyCollection<WordStatistic>> GetStatsRaw(Guid userId)
    {
        return GetStats(userId);
    }
}