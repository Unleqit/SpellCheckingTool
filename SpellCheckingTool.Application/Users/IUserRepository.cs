using SpellCheckingTool.Domain.Users;

namespace SpellCheckingTool.Application.Users;
    public interface IUserRepository
    {
        User? GetById(Guid id);
        User? GetByUsername(string username);
        void Add(User user);
    }
