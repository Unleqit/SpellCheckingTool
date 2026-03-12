using ClientApp = SpellCheckingTool.Presentation.ConsoleClient.Client;
using SpellCheckingTool.Application.Dictionary;
using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Infrastructure.Dictionary;
using SpellCheckingTool.Infrastructure.FilePersistence;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Infrastructure.UserPersistence;
using SpellCheckingTool.Infrastructure.UserSettingsPersistence;
using SpellCheckingTool.Presentation.Http.Servers;
using SpellCheckingTool.Presentation.Http.Controllers;
using SpellCheckingTool.Application.Persistence;
using SpellCheckingTool.Presentation.Servers;
using SpellCheckingTool.Infrastructure.Executables;

namespace SpellCheckingTool.Presentation;

public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("This application uses the UK Advanced Cryptics Dictionary for a predefined word list under the following license:");
        Console.WriteLine("Copyright © J Ross Beresford 1993-1999. All Rights Reserved.");
        Console.WriteLine("Visit the 'UK Advanced Cryptics Dictionary' project at: https://diginoodles.com/projects/eowl");

        //TODO: make these passable as command line arguments
        int serverPort = 12345;
        bool startHeadless = false;


        IAlphabet alphabet = new LatinAlphabet();

        var userSettingsRepository = new FileUserSettingsRepository(Path.Combine(AppContext.BaseDirectory, "data"));
        var store = new FileUserStore(Path.Combine(AppContext.BaseDirectory, "data"), alphabet, userSettingsRepository);
        var userService = new UserService(store, store, store);

        IPersistenceService persistenceService = new FilePersistenceService();

        var dictionaryLoader = new DictionaryLoader(persistenceService);
        IDefaultDictionaryProvider defaultDictionaryProvider =
            new DefaultDictionaryLoader(dictionaryLoader);

        IUserSpellcheckContextFactory spellcheckContextFactory =
            new UserSpellcheckContextFactory(defaultDictionaryProvider, userService, userSettingsRepository);

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
    }
}