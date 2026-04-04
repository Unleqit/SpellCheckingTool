using SpellCheckingTool.Application.Common;
using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Domain.WordStats;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Users;

public class UserService
{
    private readonly IUserRepository _userRepo;
    private readonly IUserWordStatsRepository _wordStatsRepo;
    private readonly IUserCustomDictionaryRepository _customDictionaryRepo;
    private readonly IUserSettingsRepository _settingsRepository;

    public UserService(
        IUserRepository userRepo,
        IUserWordStatsRepository wordStatsRepo,
        IUserCustomDictionaryRepository customDictionaryRepo,
        IUserSettingsRepository settingsRepository)
    {
        _userRepo = userRepo;
        _wordStatsRepo = wordStatsRepo;
        _customDictionaryRepo = customDictionaryRepo;
        _settingsRepository = settingsRepository;
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

        bool added = _customDictionaryRepo.AddWord(userId, word);

        if (!added)
            return OperationResult<bool>.Fail("Word already exists in personal dictionary.");

        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<bool> RemoveWord(Guid userId, string word)
    {
        if (_userRepo.GetById(userId) == null)
            return OperationResult<bool>.Fail("User not found.");

        if (string.IsNullOrWhiteSpace(word))
            return OperationResult<bool>.Fail("Word is required.");

        bool removed = _customDictionaryRepo.RemoveWord(userId, word);

        if (!removed)
            return OperationResult<bool>.Fail("Word not found in personal dictionary.");

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
}