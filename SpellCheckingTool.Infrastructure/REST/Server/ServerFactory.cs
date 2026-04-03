using SpellCheckingTool.Application.Authentication;
using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Infrastructure.Servers;
using SpellCheckingTool.Infrastructure.Http.Controllers;

namespace SpellCheckingTool.Infrastructure.Http.Servers
{
    public static class ServerFactory
    {
        public static Server Create(UserService userService, AuthService authService)
        {
            UserController userController = new UserController(userService);
            AuthController authController = new AuthController(authService);

            var server = new Server(userController, authController);


            //define logging middleware for server (like in Express.js)
            server.Use((context, next) =>
            {
                //((Console.WriteLine($"[{DateTime.Now}] {context.Request.HttpMethod} {context.Request.RawUrl}");
                next();
            });

            return server;
        }
    }
}
