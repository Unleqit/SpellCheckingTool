using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Users;

namespace TestProject.Fakes;

public class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<Guid, User> _usersById = new();
    private readonly Dictionary<string, User> _usersByName =
        new(StringComparer.OrdinalIgnoreCase);

    public User? GetById(Guid id)
    {
        _usersById.TryGetValue(id, out var user);
        return user;
    }

    public User? GetByUsername(string username)
    {
        _usersByName.TryGetValue(username, out var user);
        return user;
    }

    public void Add(User user)
    {
        _usersById[user.Id] = user;
        _usersByName[user.Username] = user;
    }

    public void Seed(User user) => Add(user);
}