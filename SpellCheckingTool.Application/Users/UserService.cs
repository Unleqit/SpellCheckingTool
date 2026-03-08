using SpellCheckingTool.Application.Common;
using SpellCheckingTool.Domain.Users;
using SpellCheckingTool.Domain.WordStats;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Users;

public class UserService
{
    private readonly IUserRepository _userRepo;
    private readonly IUserWordStatsRepository _wordStatsRepo;
    private readonly IUserCustomDictionaryRepository _customDictionaryRepo;

    public UserService(
    IUserRepository userRepo,
    IUserWordStatsRepository wordStatsRepo,
    IUserCustomDictionaryRepository customDictionaryRepo)
    {
        _userRepo = userRepo;
        _wordStatsRepo = wordStatsRepo;
        _customDictionaryRepo = customDictionaryRepo;
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

    /// <summary>
    /// Adds a word to the user's personal dictionary only.
    /// This is separate from usage statistics.
    /// </summary>
    public OperationResult<bool> AddWord(Guid userId, string word)
    {
        if (_userRepo.GetById(userId) == null)
            return OperationResult<bool>.Fail("User not found.");

        if (string.IsNullOrWhiteSpace(word))
            return OperationResult<bool>.Fail("Word is required.");

        _customDictionaryRepo.AddWord(userId, word);
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<bool> TrackWordUsage(Guid userId, string word)
    {
        if (_userRepo.GetById(userId) == null)
            return OperationResult<bool>.Fail("User not found.");

        if (string.IsNullOrWhiteSpace(word))
            return OperationResult<bool>.Fail("Word is required.");

        _wordStatsRepo.IncrementWord(userId, word);
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<IReadOnlyCollection<Word>> GetCustomWords(Guid userId)
    {
        if (_userRepo.GetById(userId) == null)
            return OperationResult<IReadOnlyCollection<Word>>.Fail("User not found.");

        var words = _customDictionaryRepo.GetWords(userId);
        return OperationResult<IReadOnlyCollection<Word>>.Ok(words);
    }

    public OperationResult<IReadOnlyCollection<WordStatistic>> GetStats(Guid userId)
    {
        if (_userRepo.GetById(userId) == null)
            return OperationResult<IReadOnlyCollection<WordStatistic>>.Fail("User not found.");

        var stats = _wordStatsRepo.GetWordStats(userId);
        return OperationResult<IReadOnlyCollection<WordStatistic>>.Ok(stats);
    }

    //to be deleted
    public OperationResult<IReadOnlyCollection<WordStatistic>> GetStatsRaw(Guid userId)
    {
        return GetStats(userId);
    }
}