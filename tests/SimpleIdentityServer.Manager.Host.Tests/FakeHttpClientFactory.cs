using Microsoft.AspNetCore.TestHost;
using SimpleIdentityServer.Common.Client.Factories;
using System.Net.Http;

namespace SimpleIdentityServer.Manager.Host.Tests
{
    public class FakeHttpClientFactory : IHttpClientFactory
    {
        private TestServer _server;

        public void Set(TestServer server)
        {
            _server = server;
        }

        public HttpClient GetHttpClient()
        {
            return _server.CreateClient();
        }
    }
}
