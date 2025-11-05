using System.Net;

namespace SpellCheckingTool
{
    public delegate void Middleware(HttpListenerContext ctx, Action next);
}
