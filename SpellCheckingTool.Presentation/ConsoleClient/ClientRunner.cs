using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool.Presentation.ConsoleClient
{
    public static class ClientRunner
    {
        public static Thread Start(int port, IUserSpellcheckContextFactory factory, CancellationTokenSource cts)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    Client.StartClient(port, factory, cts.Token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Client crashed: {ex.Message}");
                    cts.Cancel();
                }
            });

            thread.Start();
            return thread;
        }
    }
}
