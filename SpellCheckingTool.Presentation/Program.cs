using SpellCheckingTool.Application.Dictionary;
using SpellCheckingTool.Application.Persistence;
using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Infrastructure.Dictionary;
using SpellCheckingTool.Infrastructure.FilePersistence;
using SpellCheckingTool.Infrastructure.UserPersistence;
using SpellCheckingTool.Infrastructure.UserSettingsPersistence;
using SpellCheckingTool.Presentation.Composition;
using SpellCheckingTool.Presentation.ConsoleClient;
using SpellCheckingTool.Presentation.Http.Controllers;
using SpellCheckingTool.Presentation.Http.Servers;
using SpellCheckingTool.Presentation.Servers;
using System.Net;
using System.Net.Sockets;

namespace SpellCheckingTool.Presentation;

public class Program
{
    static void Main(string[] args)
    {
       PrintLicense();

        int serverPort = ParsePortFromArgs(args) ?? GetFreePort();
        bool startHeadless = args.Contains("--headless");

        var cts = SetupShutdownHandling();

        var factory = new ApplicationFactory();
        var userService = factory.CreateUserService();
        var spellcheckFactory = factory.CreateSpellcheckFactory(userService);

        var server = ServerFactory.Create(userService);
        server.Start(serverPort);

        Thread? clientThread = null;

        if (!startHeadless)
        {
            clientThread = ClientRunner.Start(serverPort, spellcheckFactory, cts);
        }
        else
        {
            Console.WriteLine("Headless mode active. Press Ctrl+C to terminate.");
        }

        Console.WriteLine("Warte auf Shutdown-Signal...");
        cts.Token.WaitHandle.WaitOne();

        Console.WriteLine("Signal erhalten. Stoppe Server...");
        server.Stop();

        Console.WriteLine("Server gestoppt. Warte auf Client-Thread...");
        if (clientThread?.IsAlive == true)
            clientThread.Join();
    }

    private static void PrintLicense()
    {
        Console.WriteLine("This application uses the UK Advanced Cryptics Dictionary for a predefined word list under the following license:");
        Console.WriteLine("Copyright © J Ross Beresford 1993-1999. All Rights Reserved.");
        Console.WriteLine("Visit the 'UK Advanced Cryptics Dictionary' project at: https://diginoodles.com/projects/eowl");

    }
    private static int? ParsePortFromArgs(string[] args)
    {
        var portArg = args.FirstOrDefault(a => a.StartsWith("--port="));
        if (portArg != null && int.TryParse(portArg.Split('=')[1], out int port))
        {
            return port;
        }
        return null;
    }

    private static int GetFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static CancellationTokenSource SetupShutdownHandling()
    {
        var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("Shutting down...");
            cts.Cancel();
        };

        return cts;
    }
}