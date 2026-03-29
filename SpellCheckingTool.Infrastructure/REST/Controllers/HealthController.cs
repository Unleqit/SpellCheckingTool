using SpellCheckingTool.Infrastructure.Http.Controllers;
using SpellCheckingTool.Infrastructure.Http.Servers.Attributes;
using System.Net;

namespace SpellCheckingTool.Infrastructure.Controller;

public class HealthController: BaseController
{
    [HttpGet("/api/v1/healthcheck")]
    public void Healthcheck(HttpListenerContext context)
    {
        // nothing to do here
        // returning 200 is handled by the framework default in Server
    }
}