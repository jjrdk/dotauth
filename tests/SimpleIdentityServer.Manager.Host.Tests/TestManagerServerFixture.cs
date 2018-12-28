namespace SimpleIdentityServer.Manager.Host.Tests
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Net.Http;

    public class TestManagerServerFixture : IDisposable
    {
        public TestServer Server { get; }
        public HttpClient Client { get; }

        public TestManagerServerFixture()
        {
            var startup = new FakeManagerStartup();
            Server = new TestServer(new WebHostBuilder()
                .UseUrls("http://localhost:5000")
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IStartup>(startup);
                })
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(FakeManagerStartup).Assembly.FullName));
            Client = Server.CreateClient();
        }

        public void Dispose()
        {
            Server.Dispose();
            Client.Dispose();
        }
    }
}
