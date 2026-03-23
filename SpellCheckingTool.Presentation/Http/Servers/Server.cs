using SpellCheckingTool.Presentation.Http.Servers;
using SpellCheckingTool.Presentation.Servers.Exceptions;
using System.Net;
using System.Reflection;

namespace SpellCheckingTool.Presentation.Servers;

public class Server
{
    private readonly HttpListener listener;
    private readonly MiddlewarePipeline middlewares;
    private readonly Router router;
    private readonly Thread requestThread;

    private bool isRequestLoopRunning;
    public bool IsStarted { get; private set; }

    /// <summary>
    /// Initializes the server
    /// </summary>
    public Server()
    {
        listener = new HttpListener();
        middlewares = new MiddlewarePipeline();

        Assembly assembly = typeof(Server).Assembly;
        router = new Router(assembly);

        requestThread = new Thread(HandleRequests)
        {
            IsBackground = true
        };
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

        try
        {
            listener.Stop();
            listener.Close();
        }
        catch (HttpListenerException ex)
        {
            Console.WriteLine(new ServerShutdownException(
                "The HTTP listener failed while shutting down.", ex).Message);
        }
        catch (ObjectDisposedException ex)
        {
            Console.WriteLine(new ServerShutdownException(
                "The HTTP listener was already disposed during shutdown.", ex).Message);
        }

        try
        {
            if (requestThread.IsAlive)
                requestThread.Join(TimeSpan.FromSeconds(2));
        }
        catch (ThreadStateException ex)
        {
            Console.WriteLine(new ServerShutdownException(
                "The request thread was not in a valid state to join.", ex).Message);
        }
    }

    /// <summary>
    /// Registers middleware in order, like Express.js
    /// </summary>
    public void Use(Middleware middleware)
    {
        middlewares.Use(middleware);
    }

    /// <summary>
    /// Handles incoming requests, runs middleware, and calls route handlers.
    /// </summary>
    private void HandleRequests()
    {
        IsStarted = true;

        while (isRequestLoopRunning)
        {
            HttpListenerContext context;

            try
            {
                context = listener.GetContext();
            }
            catch (HttpListenerException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }

            if (context.Request.Url == null)
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
                continue;
            }

            string path = context.Request.Url.AbsolutePath.ToLowerInvariant();
            string method = context.Request.HttpMethod;

            var handler = router.TryGetRoute(method, path);

            if (handler != null)
            {
                context.Response.StatusCode = 200;
                middlewares.Execute(context, () => handler(context));
            }
            else
            {
                middlewares.Execute(context, () => { });
                context.Response.StatusCode = 404;
            }

            context.Response.Close();
        }
    }
}