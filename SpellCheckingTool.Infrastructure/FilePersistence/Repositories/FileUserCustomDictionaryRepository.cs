using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.Exceptions;
using SpellCheckingTool.Domain.WordTree;
using SpellCheckingTool.Infrastructure.FilePersistence.Mappers;
using SpellCheckingTool.Infrastructure.UserPersistence;
using SpellCheckingTool.Infrastructure.UserPersistence.Models;

namespace SpellCheckingTool.Infrastructure.FilePersistence.Repositories;

public class FileUserCustomDictionaryRepository : IUserCustomDictionaryRepository
{
    private readonly object _lock = new();
    private readonly string _path;
    private readonly IAlphabet _alphabet;
    private readonly IUserRepository _userRepository;
    private readonly UserStoreJsonSerializer _serializer;
    private readonly CustomDictionaryStorageMapper _mapper;

    private Dictionary<Guid, HashSet<Word>> _userCustomDictionary;

    public FileUserCustomDictionaryRepository(
    UserStorePaths paths,
    IAlphabet alphabet,
    IUserRepository userRepository,
    UserStoreJsonSerializer serializer)
    {
        _path = paths.CustomDictionaryFilePath;
        _alphabet = alphabet;
        _userRepository = userRepository;
        _serializer = serializer;
        _mapper = new CustomDictionaryStorageMapper(alphabet);

        var storage = _serializer.ReadOrDefault(_path, new CustomDictionaryDto());
        _userCustomDictionary = _mapper.ToDomain(storage).Data;
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
        var storage = _mapper.ToStorage(new CustomDictionary
        {
            Data = _userCustomDictionary
        });

        _serializer.Write(_path, storage);
    }
}