using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool
{
    public static class UserController
    {
        private static readonly FileUserStore _store =
            new(Path.Combine(AppContext.BaseDirectory, "data"));

        private static readonly UserService _service = new(_store, _store);

        private static void WriteJson(HttpListenerContext context, int statusCode, object payload)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            using var writer = new StreamWriter(context.Response.OutputStream);
            var json = JsonConvert.SerializeObject(payload, Formatting.Indented);
            writer.Write(json);
        }

        private static void WriteError(HttpListenerContext context, int statusCode, string message)
        {
            WriteJson(context, statusCode, new { error = message });
        }

        //Authentication

        [HttpPost("/api/v1/users/register")]
        public static void Register(
           HttpListenerContext context,
           [FromBody] string username,
           [FromBody] string password)
        {
            var result = _service.Register(username, password);
            if (!result.Success || result.Value == null)
            {
                WriteError(context, 400, result.ErrorMessage ?? "Registration failed.");
                return;
            }

            WriteJson(context, 200, new
            {
                userId = result.Value.Id,
                username = result.Value.Username,
                createdAt = result.Value.CreatedAt
            });
        }


        [HttpPost("/api/v1/users/login")]
        public static void Login(
            HttpListenerContext context,
            [FromBody] string username,
            [FromBody] string password)
        {
            var result = _service.Login(username, password);
            if (!result.Success || result.Value == null)
            {
                WriteError(context, 401, result.ErrorMessage ?? "Login failed.");
                return;
            }

            WriteJson(context, 200, new
            {
                userId = result.Value.Id,
                username = result.Value.Username
            });
        }
    }
}
