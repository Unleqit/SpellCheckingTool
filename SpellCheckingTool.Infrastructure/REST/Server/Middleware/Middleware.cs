using System.Net;

namespace SpellCheckingTool.Infrastructure.Http.Servers;

    public delegate void Middleware(HttpListenerContext ctx, Action next);
