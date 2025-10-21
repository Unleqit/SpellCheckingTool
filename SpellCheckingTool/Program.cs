using System.Runtime.CompilerServices;

namespace SpellCheckingTool
{
    public unsafe class Program
    {
        static void Main(string[] args)
        {
            //TODO: make these passable as command line arguments
            int serverPort = 12345;
            bool startHeadless = false;

            //start REST server backend
            Server server = new Server(serverPort);
            
            //start CLI 'frontend' and connect it to the backend, if desired
            if (!startHeadless)
                Client.StartClient(serverPort);
        }
    }
}
