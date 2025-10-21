using System.Net;

namespace SpellCheckingTool
{
    public class Server
    {
        HttpListener listener;

        //list of all endpoints of the server API and which method is invoked upon calling them
        readonly Dictionary<string, Action<HttpListenerContext>> routes = new Dictionary<string, Action<HttpListenerContext>>
        {
            { "/api/v1/healthcheck", (context) => HandleHealthcheck(context) },
        };

        public Server(int port)
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:" + port + "/");
            listener.Start();
            Console.WriteLine("Listening on http://localhost:" + port);

            //await incoming requests inside this loop
            while (true)
            {
                var context = listener.GetContext();

                //drop empty requests (shouldn't happen)
                if (context.Request.Url == null)
                    continue;

                //select which method to invoke for this request
                if (routes.TryGetValue(context.Request.Url.AbsolutePath.ToLower(), out var handler))
                {
                    context.Response.StatusCode = 200;
                    handler(context);
                }
                else
                    context.Response.StatusCode = 404;

                //send the response back to the client
                context.Response.Close();
            }
        }

        public void Stop()
        {
            listener.Stop();
            listener.Close();
        }

        //TODO: move these handlers somewhere else
        static void HandleHealthcheck(HttpListenerContext context)
        {
            //nothing to do here, status code 200 is returned already in the above method
        }
    }
}
