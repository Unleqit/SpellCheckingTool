using Newtonsoft.Json;
using System.Net;

namespace SpellCheckingTool.Infrastructure.Http.Controllers
{
    public abstract class BaseController
    {
        protected void WriteJson(HttpListenerContext context, int statusCode, object payload)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            using var writer = new StreamWriter(context.Response.OutputStream);
            var json = JsonConvert.SerializeObject(payload, Formatting.Indented);
            writer.Write(json);
        }

        protected void WriteError(HttpListenerContext context, int statusCode, string message)
        {
            WriteJson(context, statusCode, new { error = message });
        }
    }
}
