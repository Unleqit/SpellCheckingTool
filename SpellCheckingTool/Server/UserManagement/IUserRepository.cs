using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool
{
    public interface IUserRepository
    {
        User? GetById(Guid id);
        User? GetByUsername(string username);
        void Add(User user);
    }
}
