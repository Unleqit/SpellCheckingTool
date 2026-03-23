using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Presentation.Http.Controllers;
using SpellCheckingTool.Presentation.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool.Presentation.Http.Servers
{
    public static class ServerFactory
    {
        public static Server Create(UserService userService)
        {
            UserController.Configure(userService);

            var server = new Server();

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
