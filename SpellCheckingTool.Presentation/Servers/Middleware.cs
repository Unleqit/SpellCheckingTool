using System.Net;

namespace SpellCheckingTool.Presentation.Servers;

    public delegate void Middleware(HttpListenerContext ctx, Action next);
