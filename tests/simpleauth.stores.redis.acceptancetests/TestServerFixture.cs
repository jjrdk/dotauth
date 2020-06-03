namespace SimpleAuth.Stores.Redis.AcceptanceTests
{
    using System;
    using System.Net.Http;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
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
                new WebHostBuilder().UseUrls(urls)
                    .UseConfiguration(
                        new ConfigurationBuilder().AddUserSecrets<ServerStartup>().AddEnvironmentVariables().Build())
                    .ConfigureServices(
                        services =>
                        {
                            services.AddSingleton(SharedCtx);
                            startup.ConfigureServices(services);
                        })
                    .UseSetting(WebHostDefaults.ApplicationKey, typeof(ServerStartup).Assembly.FullName)
                    .Configure(startup.Configure));
            Client = Server.CreateClient();
            SharedCtx.Client = Client;
            SharedCtx.Handler = () => Server.CreateHandler();
        }

        public void Dispose()
        {
            Server.Dispose();
            Client.Dispose();
        }
    }
}