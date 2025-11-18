using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpellCheckingTool;

namespace SpellCheckingTool
{
    public class FileUserStore : IUserRepository, IUserWordStatsRepository
    {
#pragma warning disable IL2026, IL3050
        private readonly string _usersFilePath;
        private readonly string _wordStatsFilePath;
        private readonly object _lock = new();
        private readonly IAlphabet _alphabet;

        private Dictionary<Guid, User> _users = new();
        private Dictionary<Guid, Dictionary<string, WordInfo>> _userWordStats = new();

        public FileUserStore(string baseDirectory, IAlphabet alphabet)
        {
            _alphabet = alphabet;

            Directory.CreateDirectory(baseDirectory);

            _usersFilePath = Path.Combine(baseDirectory, "users.json");
            _wordStatsFilePath = Path.Combine(baseDirectory, "wordstats.json");

            LoadUsers();
            LoadWordStats();
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
                var inner = new Dictionary<string, WordInfo>();

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
                        Word = wordEntry.Value.Statistic.Word.ToString(),   // stringify Word
                        UsageCount = wordEntry.Value.Statistic.UsageCount,
                        LastUsedAt = wordEntry.Value.Statistic.LastUsedAt
                    })
            );

            var json = JsonConvert.SerializeObject(storage, Formatting.Indented);
            File.WriteAllText(_wordStatsFilePath, json);
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
            lock(_lock)
            {
                return _users.Values.FirstOrDefault(
                    u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            }
        }

        public void Add(User user)
        {
            lock(_lock)
            {
                if (_users.ContainsKey(user.Id))
                    throw new InvalidOperationException("User with this ID already exists.");
                if (_users.Values.Any(u => u.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("Username already taken.");

                _users[user.Id] = user;

                if (!_userWordStats.ContainsKey(user.Id))
                    _userWordStats[user.Id] = new Dictionary<string, WordInfo>();

                SaveUsers();
                SaveWordStats();
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
                    words = new Dictionary<string, WordInfo>();
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

        #region Extra: expose raw words "file view"
        // This allows a “show the words file” functionality.
        public IReadOnlyDictionary<Guid, Dictionary<string, WordStatistic>> GetAllWordStatsRaw()
        {
            lock (_lock)
            {
                return _userWordStats
                    .ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value.ToDictionary(
                            inner => inner.Key,
                            inner => inner.Value.Statistic));
            }
        }

        #endregion
#pragma warning restore IL2026, IL3050
    }
}
