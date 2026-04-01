using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Infrastructure.UserSettingsPersistence;
using SpellCheckingTool.Presentation.ConsoleClient;
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
        public static Task Start(
            int port,
            IUserSpellcheckContextFactory factory,
            IFileOpener fileOpener,
            CancellationTokenSource cts)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Client.StartClient(port, factory, fileOpener, cts);
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
        }
    }
}