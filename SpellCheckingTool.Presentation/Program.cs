using ClientApp = SpellCheckingTool.Presentation.ConsoleClient.Client;
using SpellCheckingTool.Application.Dictionary;
using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Infrastructure.Dictionary;
using SpellCheckingTool.Infrastructure.FilePersistence;
using SpellCheckingTool.Infrastructure.Suggestions;
using SpellCheckingTool.Infrastructure.UserPersistence;
using SpellCheckingTool.Presentation.Http.Servers;
using SpellCheckingTool.Presentation.Http.Controllers;
using SpellCheckingTool.Application.Persistence;
using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Application.Users;

namespace SpellCheckingTool.Presentation;

public class Program
{
    static void Main(string[] args)
    {
        //Display copyright notices
        Console.WriteLine("This application uses the UK Advanced Cryptics Dictionary for a predefined word list under the following license:");
        Console.WriteLine("Copyright © J Ross Beresford 1993-1999. All Rights Reserved.");
        Console.WriteLine("Visit the 'UK Advanced Cryptics Dictionary' project at: https://diginoodles.com/projects/eowl");

        //TODO: make these passable as command line arguments
        int serverPort = 12345;
        bool startHeadless = false;

        // ----------------------------------------------------
        // Composition root (Clean Architecture wiring)
        // All infrastructure + application dependencies
        // are created and wired here in ONE place.
        // ----------------------------------------------------

        IAlphabet alphabet = new LatinAlphabet();

        var store = new FileUserStore(Path.Combine(AppContext.BaseDirectory, "data"), alphabet);
        var userService = new UserService(store, store);

        // persistence
        IPersistenceService persistenceService = new FilePersistenceService();

        // Application dictionary loader (only loads from a given FilePath)
        IDictionaryLoader dictionaryLoader = new DictionaryLoader(persistenceService);

        // Infrastructure default dictionary loader (decides where the default file is)
        var defaultDictionaryLoader = new DefaultDictionaryLoader(dictionaryLoader);

        // Inject dependencies into controller
        UserController.Configure(userService);

        // ----------------------------------------------------

        //create a server backend component
        Server server = new Server();

        //define logging middleware for server (like in Express.js)
        server.Use((context, next) =>
        {
            Console.WriteLine($"[{DateTime.Now}] {context.Request.HttpMethod} {context.Request.RawUrl}");
            next();
        });

        //start the server on a desired port
        server.Start(serverPort);

        //start CLI 'frontend' and connect it to the backend, if desired
        if (!startHeadless)
        {
            new Thread(() =>
            {
                // Load dictionary once for the client
                var tree = defaultDictionaryLoader.LoadDefaultDictionary();

                // Infrastructure suggestion implementation
                ISuggestionService suggestionService =
                    new SuggestionService(tree, new LevenshteinDistanceAlgorithm(tree));

                // Application use-case
                ISpellcheckService spellcheckService =
                    new SpellcheckService(tree, suggestionService);

                ClientApp.StartClient(serverPort, spellcheckService);
            }).Start();
        }
    }
}