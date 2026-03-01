#pragma warning disable SYSLIB0014

using SpellCheckingTool.Presentation.Http.Servers;

using System.Net;

namespace TestProject.Integration;

[TestClass]
public class ServerIntegrationTests
{
    [TestMethod]
    public void CheckIfServerIsResponding_ShouldReturn200()
    {
        const int port = 12345;

        var server = new Server();
        var serverThread = new Thread(() => server.Start(port))
        {
            IsBackground = true
        };
        serverThread.Start();

        // Wait for server startup (no infinite loop)
        bool started = SpinWait.SpinUntil(() => server.IsStarted, TimeSpan.FromSeconds(3));
        Assert.IsTrue(started, "Server did not start within timeout.");

        try
        {
            var request = (HttpWebRequest)WebRequest.CreateHttp($"http://localhost:{port}/api/v1/healthcheck");
            using var response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
        finally
        {
            server.Stop();
        }
    }
}