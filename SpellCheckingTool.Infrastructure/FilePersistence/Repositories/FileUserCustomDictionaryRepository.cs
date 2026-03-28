using SpellCheckingTool.Infrastructure.UserPersistence;

namespace SpellCheckingTool.Infrastructure.FilePersistence.Repositories;

using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.Exceptions;
using SpellCheckingTool.Domain.WordTree;
using SpellCheckingTool.Infrastructure.FilePersistence;
using SpellCheckingTool.Infrastructure.FilePersistence.Mappers;

public class FileUserCustomDictionaryRepository : IUserCustomDictionaryRepository
{
    private readonly object _lock = new();
    private readonly string _path;
    private readonly IAlphabet _alphabet;
    private readonly IUserRepository _userRepository;
    private readonly UserStoreJsonReader _reader;
    private readonly UserStoreJsonWriter _writer;
    private readonly CustomDictionaryStorageMapper _mapper;

    private Dictionary<Guid, HashSet<Word>> _userCustomDictionary;

    public FileUserCustomDictionaryRepository(
        UserStorePaths paths,
        IAlphabet alphabet,
        IUserRepository userRepository,
        UserStoreJsonReader reader,
        UserStoreJsonWriter writer)
    {
        _path = paths.CustomDictionaryFilePath;
        _alphabet = alphabet;
        _userRepository = userRepository;
        _reader = reader;
        _writer = writer;
        _mapper = new CustomDictionaryStorageMapper(alphabet);

        var storage = _reader.ReadOrDefault(_path, new Dictionary<Guid, List<string>>());
        _userCustomDictionary = _mapper.ToDomain(storage);
    }

    public void AddWord(Guid userId, string word)
    {
        lock (_lock)
        {
            if (_userRepository.GetById(userId) == null)
                throw new UserNotFoundDomainException(userId);

            var normalized = word.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalized))
                return;

            if (!_userCustomDictionary.TryGetValue(userId, out var words))
            {
                words = new HashSet<Word>();
                _userCustomDictionary[userId] = words;
            }

            words.Add(new Word(_alphabet, normalized));
            Save();
        }
    }

    public bool RemoveWord(Guid userId, string word)
    {
        lock (_lock)
        {
            if (_userRepository.GetById(userId) == null)
                throw new UserNotFoundDomainException(userId);

            if (!_userCustomDictionary.TryGetValue(userId, out var words))
                return false;

            var normalized = word.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalized))
                return false;

            var removed = words.Remove(new Word(_alphabet, normalized));
            if (removed)
                Save();

            return removed;
        }
    }

    public IReadOnlyCollection<Word> GetWords(Guid userId)
    {
        lock (_lock)
        {
            if (_userRepository.GetById(userId) == null)
                throw new UserNotFoundDomainException(userId);

            if (!_userCustomDictionary.TryGetValue(userId, out var words))
                return Array.Empty<Word>();

            return words
                .OrderBy(w => w.ToString(), StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }

    private void Save()
    {
        var storage = _mapper.ToStorage(_userCustomDictionary);
        _writer.Write(_path, storage);
    }
}