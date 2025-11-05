#pragma warning disable SYSLIB0014

using SpellCheckingTool;
using System.Net;

namespace TestProject.Integration
{
    [TestClass]
    public class ServerIntegrationTests
    {
        [TestMethod]
        public void CheckIfServerIsResponding_ShouldReturn200()
        {
            Server server = new Server();
            Thread serverThread = new Thread(() => server.Start(12345));
            serverThread.Start();

            //await startup of the server
            while (!server.IsStarted) ;

            try
            {
                var clientRequest = (HttpWebRequest)HttpWebRequest.CreateHttp("http://localhost:12345/api/v1/healthcheck");
                var serverResponse = (HttpWebResponse)clientRequest.GetResponse();

                if (serverResponse.StatusCode != HttpStatusCode.OK)
                    Assert.Fail(serverResponse.StatusCode.ToString());

            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            } 
        }
    }
}