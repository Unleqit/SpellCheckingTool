using SpellCheckingTool.Application.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool.Presentation.ConsoleClient.ClientServices
{
    public class ClientUserService
    {
        public ClientAuthService Auth { get; }
        public ClientWordService Words { get; }
        public ClientStatsService Stats { get; }

        public ClientUserService(ClientAuthService auth, ClientWordService words, ClientStatsService stats)
        {
            Auth = auth;
            Words = words;
            Stats = stats;
        }
    }
}
