using SpellCheckingTool.Infrastructure.UserPersistence;

namespace SpellCheckingTool.Infrastructure.FilePersistence.Repositories;

using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Exceptions;
using SpellCheckingTool.Domain.Users;

public class FileUserRepository : IUserRepository
{
    private readonly object _lock = new();
    private readonly string _path;
    private readonly UserStoreJsonSerializer _serializer;
    private Dictionary<Guid, User> _users;

    public FileUserRepository(
    UserStorePaths paths,
    UserStoreJsonSerializer serializer)
    {
        _path = paths.UsersFilePath;
        _serializer = serializer;
        _users = _serializer.ReadOrDefault(_path, new Dictionary<Guid, User>());
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
            _serializer.Write(_path, _users);
        }
    }
}