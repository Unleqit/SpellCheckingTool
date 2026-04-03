using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Presentation.ConsoleClient.Exceptions;

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