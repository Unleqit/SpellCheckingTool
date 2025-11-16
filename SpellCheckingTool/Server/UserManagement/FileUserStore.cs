using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool
{
    public class FileUserStore : IUserRepository, IUserWordStatsRepository
    {
        private readonly string _usersFilePath;
        private readonly string _wordStatsFilePath;
        private readonly object _lock = new();

        private Dictionary<Guid, User> _users = new();
        private Dictionary<Guid, Dictionary<string, WordStatistic>> _userWordStats = new();

        public FileUserStore(string baseDirectory)
        {
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
                _userWordStats = new Dictionary<Guid, Dictionary<string, WordStatistic>>();
                return;
            }

            var json = File.ReadAllText(_wordStatsFilePath);
            _userWordStats =
                JsonConvert.DeserializeObject<Dictionary<Guid, Dictionary<string, WordStatistic>>>(json)
                ?? new Dictionary<Guid, Dictionary<string, WordStatistic>>();
        }

        private void SaveUsers()
        {
            var json = JsonConvert.SerializeObject(_users, Formatting.Indented);
            File.WriteAllText(_usersFilePath, json);
        }

        private void SaveWordStats()
        {
            var json = JsonConvert.SerializeObject(_userWordStats, Formatting.Indented);
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
                    _userWordStats[user.Id] = new Dictionary<string, WordStatistic>();

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
                    words = new Dictionary<string, WordStatistic>();
                    _userWordStats[userId] = words;
                }

                var normalized = word.Trim().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(normalized))
                    return;

                if (!words.TryGetValue(normalized, out var stat))
                {
                    stat = new WordStatistic(normalized);
                    words[normalized] = stat;
                }

                stat.Increment();

                SaveWordStats();
            }
        }

        public IReadOnlyCollection<WordStatistic> GetWordStats(Guid userId)
        {
            lock (_lock)
            {
                if (!_userWordStats.TryGetValue(userId, out var dict))
                    return Array.Empty<WordStatistic>();

                // return a copy so callers can't mess it up
                return dict.Values
                           .Select(ws => new WordStatistic
                           {
                               Word = ws.Word,
                               UsageCount = ws.UsageCount,
                               LastUsedAt = ws.LastUsedAt
                           })
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
                            inner => new WordStatistic
                            {
                                Word = inner.Value.Word,
                                UsageCount = inner.Value.UsageCount,
                                LastUsedAt = inner.Value.LastUsedAt
                            }));
            }
        }

        #endregion

    }
}
