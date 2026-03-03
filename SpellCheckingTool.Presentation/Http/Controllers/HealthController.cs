using SpellCheckingTool.Presentation.Http.Servers.Attributes;
using System.Net;

namespace SpellCheckingTool.Presentation.Controller;

public static class HealthController
{
    [HttpGet("/api/v1/healthcheck")]
    public static void Healthcheck(HttpListenerContext context)
    {
        // nothing to do here
        // returning 200 is handled by the framework default in Server
    }
}