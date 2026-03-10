using Newtonsoft.Json;
using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordStats;
using SpellCheckingTool.Domain.WordTree;
using SpellCheckingTool.Domain.Users;

namespace SpellCheckingTool.Infrastructure.UserPersistence;

public class FileUserStore : IUserRepository, IUserWordStatsRepository, IUserCustomDictionaryRepository
{
    private readonly string _usersFilePath;
    private readonly string _wordStatsFilePath;
    private readonly string _customDictionaryFilePath;
    private readonly object _lock = new();
    private readonly IAlphabet _alphabet;

    private Dictionary<Guid, User> _users = new();
    private Dictionary<Guid, Dictionary<string, WordInfo>> _userWordStats = new();
    private Dictionary<Guid, HashSet<Word>> _userCustomDictionary = new();


    public FileUserStore(string baseDirectory, IAlphabet alphabet)
    {
        _alphabet = alphabet;

        Directory.CreateDirectory(baseDirectory);

        _usersFilePath = Path.Combine(baseDirectory, "users.json");
        _wordStatsFilePath = Path.Combine(baseDirectory, "wordstats.json");
        _customDictionaryFilePath = Path.Combine(baseDirectory, "userdictionary.json");

        LoadUsers();
        LoadWordStats();
        LoadCustomDictionary();
    }

    #region Load / Save

    private void LoadUsers()
    {
        if (!File.Exists(_usersFilePath))
        {
            _users = new Dictionary<Guid, User>();
            return;
        }

        var json = File.ReadAllText(_usersFilePath);
        _users = JsonConvert.DeserializeObject<Dictionary<Guid, User>>(json)
                 ?? new Dictionary<Guid, User>();
    }

    private void LoadWordStats()
    {
        if (!File.Exists(_wordStatsFilePath))
        {
            _userWordStats = new Dictionary<Guid, Dictionary<string, WordInfo>>();
            return;
        }

        var json = File.ReadAllText(_wordStatsFilePath);

        var storage =
            JsonConvert.DeserializeObject<Dictionary<Guid, Dictionary<string, WordStatisticStorage>>>(json)
            ?? new Dictionary<Guid, Dictionary<string, WordStatisticStorage>>();

        var result = new Dictionary<Guid, Dictionary<string, WordInfo>>();

        foreach (var userEntry in storage)
        {
            var inner = new Dictionary<string, WordInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var wordEntry in userEntry.Value)
            {
                var storageStat = wordEntry.Value;
                    // recreate Word using the alphabet
                var wordObj = new Word(_alphabet, storageStat.Word);

                var stat = new WordStatistic(
                    wordObj,
                    storageStat.UsageCount,
                    storageStat.LastUsedAt
                );

                inner[wordEntry.Key] = new WordInfo(wordEntry.Key, stat);
            }

            result[userEntry.Key] = inner;
        }

        _userWordStats = result;
    }

    private void LoadCustomDictionary()
    {
        if (!File.Exists(_customDictionaryFilePath))
        {
            _userCustomDictionary = new Dictionary<Guid, HashSet<Word>>();
            return;
        }

        var json = File.ReadAllText(_customDictionaryFilePath);
        var storage =
            JsonConvert.DeserializeObject<Dictionary<Guid, List<string>>>(json)
            ?? new Dictionary<Guid, List<string>>();

        var result = new Dictionary<Guid, HashSet<Word>>();

        foreach (var userEntry in storage)
        {
            var words = new HashSet<Word>();

            foreach (var raw in userEntry.Value)
            {
                var normalized = raw?.Trim().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(normalized))
                    continue;

                try
                {
                    words.Add(new Word(_alphabet, normalized));
                }
                catch
                {
                    // optional logging
                }
            }

            result[userEntry.Key] = words;
        }

        _userCustomDictionary = result;
    }

    private void SaveUsers()
        {
            var json = JsonConvert.SerializeObject(_users, Formatting.Indented);
            File.WriteAllText(_usersFilePath, json);
        }

    private void SaveWordStats()
    {
        var storage = _userWordStats.ToDictionary(
            userEntry => userEntry.Key,
            userEntry => userEntry.Value.ToDictionary(
                wordEntry => wordEntry.Key,
                wordEntry => new WordStatisticStorage
                {
                    Word = wordEntry.Value.Statistic.Word.ToString(),
                    UsageCount = wordEntry.Value.Statistic.UsageCount,
                    LastUsedAt = wordEntry.Value.Statistic.LastUsedAt
                    })
            );

        var json = JsonConvert.SerializeObject(storage, Formatting.Indented);
        File.WriteAllText(_wordStatsFilePath, json);
    }

    private void SaveCustomDictionary()
    {
        var storage = _userCustomDictionary.ToDictionary(
            userEntry => userEntry.Key,
            userEntry => userEntry.Value
                .Select(w => w.ToString())
                .OrderBy(w => w, StringComparer.OrdinalIgnoreCase)
                .ToList());

        var json = JsonConvert.SerializeObject(storage, Formatting.Indented);
        File.WriteAllText(_customDictionaryFilePath, json);
    }

    #endregion

    #region IUserRepository

    public User? GetById(Guid id)
    {
        lock (_lock)
        {
            _users.TryGetValue(id, out var user);
            return user;
        }
    }

    public User? GetByUsername(string username)
    {
        lock (_lock)
        {
            return _users.Values.FirstOrDefault(
                u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
    }

    public void Add(User user)
    {
        lock (_lock)
        {
            if (_users.ContainsKey(user.Id))
                throw new InvalidOperationException("User with this ID already exists.");

            if (_users.Values.Any(u => u.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Username already taken.");

            _users[user.Id] = user;

            if (!_userWordStats.ContainsKey(user.Id))
                _userWordStats[user.Id] = new Dictionary<string, WordInfo>(StringComparer.OrdinalIgnoreCase);

            if (!_userCustomDictionary.ContainsKey(user.Id))
                _userCustomDictionary[user.Id] = new HashSet<Word>();

            SaveUsers();
            SaveWordStats();
            SaveCustomDictionary();
        }
    }

    #endregion

    #region IUserWordStatsRepository

    public void IncrementWord(Guid userId, string word)
    {
        lock (_lock)
        {
            if (!_users.ContainsKey(userId))
                throw new KeyNotFoundException("User not found.");

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
                var stat = new WordStatistic(wordObj);
                info = new WordInfo(normalized, stat);
                words[normalized] = info;
            }

            info.Statistic.Increment();
            SaveWordStats();
        }
    }

    public IReadOnlyCollection<WordStatistic> GetWordStats(Guid userId)
    {
        lock (_lock)
        {
            if (!_userWordStats.TryGetValue(userId, out var dict))
                return Array.Empty<WordStatistic>();

            return dict.Values
                .Select(info => info.Statistic)
                .ToList()
                .AsReadOnly();
        }
    }

    #endregion

    #region IUserCustomDictionaryRepository

    private string? NormalizeAndValidate(string word)
    {
        var normalized = word.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        _ = new Word(_alphabet, normalized);
        return normalized;
    }

    public void AddWord(Guid userId, string word)
    {
        lock (_lock)
        {
            if (!_users.ContainsKey(userId))
                throw new KeyNotFoundException("User not found.");

            if (!_userCustomDictionary.TryGetValue(userId, out var words))
            {
                words = new HashSet<Word>();
                _userCustomDictionary[userId] = words;
            }

            var normalized = word.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalized))
                return;

            words.Add(new Word(_alphabet, normalized));
            SaveCustomDictionary();
        }
    }

    public bool RemoveWord(Guid userId, string word)
    {
        lock (_lock)
        {
            if (!_users.ContainsKey(userId))
                throw new KeyNotFoundException("User not found.");

            if (!_userCustomDictionary.TryGetValue(userId, out var words))
                return false;

            var normalized = word.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalized))
                return false;

            bool removed = words.Remove(new Word(_alphabet, normalized));

            if (removed)
            {
                SaveCustomDictionary();
            }

            return removed;
        }
    }

    public IReadOnlyCollection<Word> GetWords(Guid userId)
    {
        lock (_lock)
        {
            if (!_userCustomDictionary.TryGetValue(userId, out var words))
                return Array.Empty<Word>();

            return words
                .OrderBy(w => w.ToString(), StringComparer.OrdinalIgnoreCase)
                .ToList()
                .AsReadOnly();
        }
    }

    #endregion

    #region Optional raw stats view

    public IReadOnlyDictionary<Guid, Dictionary<string, WordStatistic>> GetAllWordStatsRaw()
    {
        lock (_lock)
        {
            return _userWordStats.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.ToDictionary(
                    inner => inner.Key,
                    inner => inner.Value.Statistic));
        }
    }

    #endregion
}
