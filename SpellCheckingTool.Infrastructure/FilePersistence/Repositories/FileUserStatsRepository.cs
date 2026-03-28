using SpellCheckingTool.Infrastructure.UserPersistence;

namespace SpellCheckingTool.Infrastructure.FilePersistence.Repositories;

using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.Exceptions;
using SpellCheckingTool.Domain.WordStats;
using SpellCheckingTool.Domain.WordTree;
using SpellCheckingTool.Infrastructure.FilePersistence;
using SpellCheckingTool.Infrastructure.FilePersistence.Mappers;

public class FileUserWordStatsRepository : IUserWordStatsRepository
{
    private readonly object _lock = new();
    private readonly string _path;
    private readonly IAlphabet _alphabet;
    private readonly IUserRepository _userRepository;
    private readonly UserStoreJsonReader _reader;
    private readonly UserStoreJsonWriter _writer;
    private readonly WordStatisticStorageMapper _mapper;

    private Dictionary<Guid, Dictionary<string, WordInfo>> _userWordStats;

    public FileUserWordStatsRepository(
        UserStorePaths paths,
        IAlphabet alphabet,
        IUserRepository userRepository,
        UserStoreJsonReader reader,
        UserStoreJsonWriter writer)
    {
        _path = paths.WordStatsFilePath;
        _alphabet = alphabet;
        _userRepository = userRepository;
        _reader = reader;
        _writer = writer;
        _mapper = new WordStatisticStorageMapper(alphabet);

        var storage = _reader.ReadOrDefault(
            _path,
            new Dictionary<Guid, Dictionary<string, WordStatisticStorage>>());

        _userWordStats = _mapper.ToDomain(storage);
    }

    public void IncrementWord(Guid userId, string word)
{
    lock (_lock)
    {
        if (_userRepository.GetById(userId) == null)
            throw new UserNotFoundDomainException(userId);

        if (!_userWordStats.TryGetValue(userId, out var words))
        {
            words = new Dictionary<string, WordInfo>(StringComparer.OrdinalIgnoreCase);
            _userWordStats[userId] = words;
        }

        var normalized = word.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
            return;

        if (!words.TryGetValue(normalized, out var info))
        {
            var wordObj = new Word(_alphabet, normalized);
            var statistic = new WordStatistic(wordObj);
            statistic.Increment(); 
            words[normalized] = new WordInfo(normalized, statistic);
        }
        else
        {
            info.Statistic.Increment();
        }

        Save();
    }
}

    public IReadOnlyCollection<WordStatistic> GetWordStats(Guid userId)
    {
        lock (_lock)
        {
            if (_userRepository.GetById(userId) == null)
                throw new UserNotFoundDomainException(userId);

            if (!_userWordStats.TryGetValue(userId, out var words))
                return Array.Empty<WordStatistic>();

            return words.Values
                .Select(x => x.Statistic)
                .OrderByDescending(x => x.UsageCount)
                .ThenByDescending(x => x.LastUsedAt)
                .ToList();
        }
    }

    private void Save()
    {
        var storage = _mapper.ToStorage(_userWordStats);
        _writer.Write(_path, storage);
    }
}