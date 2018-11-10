using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace SimpleIdentityServer.Manager.Host.Tests
{
    public class TestManagerServerFixture : IDisposable
    {
        public TestServer Server { get; }
        public HttpClient Client { get; }
        public SharedContext SharedCtx { get; }

        public TestManagerServerFixture()
        {
            SharedCtx = new SharedContext();
            var startup = new FakeStartup(SharedCtx);
            Server = new TestServer(new WebHostBuilder()
                .UseUrls("http://localhost:5000")
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IStartup>(startup);
                })
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(FakeStartup).GetType().Assembly.FullName));
            Client = Server.CreateClient();
            SharedCtx.HttpClientFactory.Set(Server);
        }

        public void Dispose()
        {
            Server.Dispose();
            Client.Dispose();
        }
    }
}
