namespace SimpleAuth.Stores.Marten.AcceptanceTests
{
    using System;
    using System.Net.Http;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;

    public class TestServerFixture : IDisposable
    {
        public TestServer Server { get; }
        public HttpClient Client { get; }
        public SharedContext SharedCtx { get; }

        public TestServerFixture(string connectionString, params string[] urls)
        {
            SharedCtx = SharedContext.Instance;
            var startup = new ServerStartup(SharedCtx, connectionString);
            Server = new TestServer(
                new WebHostBuilder()
                    .UseUrls(urls)
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton<IStartup>(startup);
                    })
                    .UseSetting(WebHostDefaults.ApplicationKey, typeof(ServerStartup).Assembly.FullName));
            Client = Server.CreateClient();
            SharedCtx.Client = Client;
        }

        public void Dispose()
        {
            Server.Dispose();
            Client.Dispose();
        }
    }
}