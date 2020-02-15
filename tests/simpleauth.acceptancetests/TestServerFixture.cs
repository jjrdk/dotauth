namespace SimpleAuth.AcceptanceTests
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

        public TestServerFixture(params string[] urls)
        {
            SharedCtx = SharedContext.Instance;
            var startup = new ServerStartup(SharedCtx);
            Server = new TestServer(
                new WebHostBuilder()
                    .UseUrls(urls)
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton<IStartup>(startup);
                    })
                    .UseSetting(WebHostDefaults.ApplicationKey, typeof(ServerStartup).Assembly.FullName));
            Client = Server.CreateClient();
            SharedCtx.Client = Server.CreateClient();
            SharedCtx.Handler = Server.CreateHandler();
        }

        public void Dispose()
        {
            Server.Dispose();
            Client.Dispose();
        }
    }
}
