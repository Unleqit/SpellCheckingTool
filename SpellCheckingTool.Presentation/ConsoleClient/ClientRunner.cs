using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Presentation.ConsoleClient.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool.Presentation.ConsoleClient
{
    public static class ClientRunner
    {
        public static Thread Start(
            int port,
            IUserSpellcheckContextFactory factory,
            IFileOpener fileOpener,
            CancellationTokenSource cts)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    Client.StartClient(port, factory, fileOpener, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    cts.Cancel();
                }
                catch (Exception ex)
                {
                    var clientEx = new ClientExecutionException("The console client terminated unexpectedly.", ex);
                    Console.WriteLine(clientEx.Message);
                    cts.Cancel();
                }
            });

            thread.Start();
            return thread;
        }
    }
}
