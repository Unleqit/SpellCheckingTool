using SpellCheckingTool.Application.Dictionary;
using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Infrastructure.Dictionary;
using SpellCheckingTool.Infrastructure.Executables;
using SpellCheckingTool.Infrastructure.FilePersistence;
using SpellCheckingTool.Infrastructure.FilePersistence.Repositories;
using SpellCheckingTool.Infrastructure.UserPersistence;
using SpellCheckingTool.Infrastructure.UserSettingsPersistence;
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
        LicensePrinter.Print();

        int serverPort = ParsePortFromArgs(args) ?? GetFreePort();
        bool startHeadless = args.Contains("--headless");

        var cts = SetupShutdownHandling();

        var (userService, spellcheckFactory) = BuildApplication();

        var server = ServerFactory.Create(userService);
        server.Start(serverPort);

        var fileOpener = new FileOpener();

        Thread? clientThread = null;

        if (!startHeadless)
        {
            clientThread = ClientRunner.Start(serverPort, spellcheckFactory, fileOpener, cts);
        }
        else
        {
            Console.WriteLine("Headless mode active. Press Ctrl+C to terminate.");
        }

        cts.Token.WaitHandle.WaitOne();

        server.Stop();

        if (clientThread?.IsAlive == true)
            clientThread.Join();
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

    private static (UserService, IUserSpellcheckContextFactory) BuildApplication()
    {
        var inputAlphabet = new UTF16Alphabet();
        var basePath = Path.Combine(AppContext.BaseDirectory, "data");

        var userSettingsRepository = new FileUserSettingsRepository(
            Path.Combine(basePath, "UserSettings"));

        var paths = new UserStorePaths(basePath);
        var serializer = new UserStoreJsonSerializer();

        IUserRepository userRepository = new FileUserRepository(
            paths,
            serializer
        );

        IUserWordStatsRepository wordStatsRepository = new FileUserWordStatsRepository(
            paths,
            inputAlphabet,
            userRepository,
            serializer
        );

        IUserCustomDictionaryRepository customDictionaryRepository = new FileUserCustomDictionaryRepository(
            paths,
            inputAlphabet,
            userRepository,
            serializer
        );

        var persistenceService = new FilePersistenceService();
        var dictionaryLoader = new DictionaryLoader(persistenceService);
        var defaultDictionaryProvider = new DefaultDictionaryLoader(dictionaryLoader);

        var userService = new UserService(
            userRepository,
            wordStatsRepository,
            customDictionaryRepository,
            userSettingsRepository
        );

        var userTreeBuilder = new UserWordTreeBuilder(
            defaultDictionaryProvider,
            userService
        );


        var executableParser = new WindowsExecutableParser();
        var spellcheckFactory = new UserSpellcheckContextFactory(
            userTreeBuilder,
            userService,
            userSettingsRepository,
            inputAlphabet,
            executableParser

        );

        return (userService, spellcheckFactory);
    }
}