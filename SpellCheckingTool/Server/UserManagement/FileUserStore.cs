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
    }
}
