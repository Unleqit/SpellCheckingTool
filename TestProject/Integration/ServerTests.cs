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
            Thread serverThread = new Thread(() =>
            {
                Server server = new Server(12345);
            });
            serverThread.Start();

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