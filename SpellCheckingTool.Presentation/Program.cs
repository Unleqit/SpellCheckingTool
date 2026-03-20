using SpellCheckingTool.Application.Dictionary;
using SpellCheckingTool.Application.Persistence;
using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Infrastructure.Dictionary;
using SpellCheckingTool.Infrastructure.Executables;
using SpellCheckingTool.Infrastructure.FilePersistence;
using SpellCheckingTool.Infrastructure.UserPersistence;
using SpellCheckingTool.Infrastructure.UserSettingsPersistence;
using SpellCheckingTool.Presentation.Http.Controllers;
using SpellCheckingTool.Presentation.Http.Servers;
using SpellCheckingTool.Presentation.Servers;
using System.Net;
using System.Net.Sockets;
using ClientApp = SpellCheckingTool.Presentation.ConsoleClient.Client;

namespace SpellCheckingTool.Presentation;

public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("This application uses the UK Advanced Cryptics Dictionary for a predefined word list under the following license:");
        Console.WriteLine("Copyright © J Ross Beresford 1993-1999. All Rights Reserved.");
        Console.WriteLine("Visit the 'UK Advanced Cryptics Dictionary' project at: https://diginoodles.com/projects/eowl");

        int serverPort = ParsePortFromArgs(args) ?? GetFreePort();
        bool startHeadless = args.Contains("--headless");

        IAlphabet inputAlphabet = new UTF16Alphabet();

        var basePath = Path.Combine(AppContext.BaseDirectory, "data");

        var userSettingsRepository = new FileUserSettingsRepository(
            Path.Combine(
            basePath, "UserSettings")
            );
        var store = new FileUserStore(Path.Combine(
            AppContext.BaseDirectory, "data"),
            inputAlphabet, userSettingsRepository
            );
        var userService = new UserService(store, store, store);

        IPersistenceService persistenceService = new FilePersistenceService();

        var dictionaryLoader = new DictionaryLoader(persistenceService);
        IDefaultDictionaryProvider defaultDictionaryProvider =
            new DefaultDictionaryLoader(dictionaryLoader);

        IUserSpellcheckContextFactory spellcheckContextFactory =
            new UserSpellcheckContextFactory(defaultDictionaryProvider, userService, userSettingsRepository, inputAlphabet);

        UserController.Configure(userService);

        Server server = new Server();

        //define logging middleware for server (like in Express.js)
        server.Use((context, next) =>
        {
            //((Console.WriteLine($"[{DateTime.Now}] {context.Request.HttpMethod} {context.Request.RawUrl}");
            next();
        });

        //start the server on a desired port
        server.Start(serverPort);

        //start CLI 'frontend' and connect it to the backend, if desired
        if (!startHeadless)
        {
            new Thread(() =>
            {
                ClientApp.StartClient(serverPort, spellcheckContextFactory);
            }).Start();
        }
        else
        {
            Console.WriteLine("Headless mode active. Press Ctrl+C to terminate.");
            // Im Headless-Modus müssen wir verhindern, dass sich die App sofort beendet
            Thread.Sleep(Timeout.Infinite);
        }
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
}