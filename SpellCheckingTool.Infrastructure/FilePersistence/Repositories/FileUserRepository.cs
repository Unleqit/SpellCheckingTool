using SpellCheckingTool.Infrastructure.UserPersistence;

namespace SpellCheckingTool.Infrastructure.FilePersistence.Repositories;

using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Exceptions;
using SpellCheckingTool.Domain.Users;
using SpellCheckingTool.Infrastructure.FilePersistence;

public class FileUserRepository : IUserRepository
{
    private readonly object _lock = new();
    private readonly string _path;
    private readonly UserStoreJsonReader _reader;
    private readonly UserStoreJsonWriter _writer;
    private Dictionary<Guid, User> _users;

    public FileUserRepository(
        UserStorePaths paths,
        UserStoreJsonReader reader,
        UserStoreJsonWriter writer)
    {
        _path = paths.UsersFilePath;
        _reader = reader;
        _writer = writer;
        _users = _reader.ReadOrDefault(_path, new Dictionary<Guid, User>());
    }

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
                throw new DuplicateUserException(user.Id);

            if (_users.Values.Any(u =>
                u.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase)))
            {
                throw new DuplicateUserException(user.Username);
            }

            _users[user.Id] = user;
            _writer.Write(_path, _users);
        }
    }
}