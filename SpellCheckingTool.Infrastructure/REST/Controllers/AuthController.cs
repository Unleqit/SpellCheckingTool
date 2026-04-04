using SpellCheckingTool.Application.Authentication;
using SpellCheckingTool.Infrastructure.Http.Servers.Attributes;
using System.Net;

namespace SpellCheckingTool.Infrastructure.Http.Controllers
{
    internal class AuthController: BaseController
    {
        AuthService _service;

        public AuthController(AuthService service)
        {
            this._service = service;
        }

        [HttpPost("/api/v1/users/register")]
        public void Register(HttpListenerContext context, [FromBody] string username, [FromBody] string password)
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
        public void Login(HttpListenerContext context, [FromBody] string username, [FromBody] string password)
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

        [HttpPost("/api/v1/users/check-username")]
        public void CheckUsername(HttpListenerContext context, [FromBody] string username)
        {
            var result = _service.UsernameExists(username);

            if (!result.Success)
            {
                WriteError(context, 400, result.ErrorMessage ?? "Username check failed.");
                return;
            }

            WriteJson(context, 200, new
            {
                exists = result.Value
            });
        }
    }
}
