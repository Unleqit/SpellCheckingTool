

namespace SpellCheckingTool
{
    public unsafe class Program
    {
        static void Main(string[] args)
        {
            //TODO: make these passable as command line arguments
            int serverPort = 12345;
            bool startHeadless = false;

            //create a server backend component
            Server server = new Server();

            //define logging middleware for server (like in Express.js)
            server.Use((context, next) =>
            {
                Console.WriteLine($"[{DateTime.Now}] {context.Request.HttpMethod} {context.Request.RawUrl}");
                next();
            });

            //start the server on a desired port
            server.Start(12345);
            
            //start CLI 'frontend' and connect it to the backend, if desired
            if (!startHeadless)
                Client.StartClient(serverPort);

        }
    }
}
