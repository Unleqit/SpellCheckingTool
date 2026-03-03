using System.Net;

namespace SpellCheckingTool.Presentation.Http.Servers;

    public delegate void Middleware(HttpListenerContext ctx, Action next);
