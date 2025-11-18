using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Username { get; private set; } = "";
        public string PasswordHash { get; private set; } = "";
        public DateTime CreatedAt { get; private set; }

        private User() { }

        public User(Guid id, string username, string passwordHash, DateTime createdAt)
        {
            Id = id;
            Username = username;
            PasswordHash = passwordHash;
            CreatedAt = createdAt;
        }
    }
}
