using SpellCheckingTool.Common.Routing.Attributes;
using System.IO;
using System.Net;
using System.Reflection;

namespace SpellCheckingTool
{
    public class Server
    {
        HttpListener listener;
        MiddlewarePipeline middlewares;
        Router router;
        Thread requestThread;
        bool isRequestLoopRunning;
        public bool IsStarted { get; private set; }

        /// <summary>
        /// Initializes the server
        /// </summary>
        public Server()
        {
            listener = new HttpListener();
            middlewares = new MiddlewarePipeline();

//We load the assembly of *this* code, so it should always exist
#pragma warning disable CS8604
            router = new Router(Assembly.GetAssembly(typeof(Server)));
#pragma warning restore CS8604 

            requestThread = new Thread(HandleRequests);
        }

        /// <summary>
        /// Starts the server
        /// </summary>
        public void Start(int port)
        {
            listener.Prefixes.Add("http://localhost:" + port + "/");
            listener.Start();
            Console.WriteLine("Listening on http://localhost:" + port);
            isRequestLoopRunning = true;
            requestThread.Start();
        }

        /// <summary>
        /// Stops the server
        /// </summary>
        public void Stop()
        {
            IsStarted = false;
            isRequestLoopRunning = false;
            listener.Stop();
            listener.Close();
            //join the requestThread to ensure its completion
            requestThread.Join();
        }

        /// <summary>
        /// Registers middleware in order, like Express.js
        /// </summary>
        /// <param name="middleware">The middleware to use</param>
        public void Use(Middleware middleware)
        {
            middlewares.Use(middleware);
        }

        /// <summary>
        /// Handles incoming requests, runs middleware, and calls route handlers.
        /// </summary>
        private void HandleRequests()
        {
            string path;
            string method;
            HttpListenerContext context;
            IsStarted = true;

            while (isRequestLoopRunning)
            {
                try
                {
                    context = listener.GetContext();
                }
                catch (Exception)
                {
                    //acquiring request context failed (TODO: logging?)
                    break;
                }

                //drop empty requests
                if (context.Request.Url == null)
                    continue;

                //get path and method of the request
                path = context.Request.Url.AbsolutePath.ToLower();
                method = context.Request.HttpMethod;

                //determine the C# method to call for this request
                var handler = router.TryGetRoute(method, path);

                if (handler != null)
                {
                    context.Response.StatusCode = 200;
                    middlewares.Execute(context, () => handler(context));
                }
                else
                {
                    //requested endpoint doesn't exist (TODO: add logging?)
                    middlewares.Execute(context, () => { });
                    context.Response.StatusCode = 404;
                }

                //send the response back to the client
                context.Response.Close();
            }
        }

        [HttpGet("/api/v1/healthcheck")]
        public static void HandleHealthcheck(HttpListenerContext context)
        {
            //nothing to do here, status 200 returned by default
        }

        //To test this, make a POST request at this endpoint and put this in the request body:   {"testString":"Hello from POST request"}
        [HttpPost("/api/v1/jsonexample")]
        public static void TestFromBodyJsonAttribute([FromBody] string testString)
        {
            Console.WriteLine(testString);
        }
    }
}
