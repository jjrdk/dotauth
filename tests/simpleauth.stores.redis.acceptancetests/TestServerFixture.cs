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
            Server = new TestServer(
                new WebHostBuilder().UseUrls(urls)
                    .UseConfiguration(
                        new ConfigurationBuilder().AddUserSecrets<ServerStartup>().AddEnvironmentVariables().Build())
                    .ConfigureServices(
                        services =>
                        {
                            services.AddSingleton(SharedCtx);
                            services.AddSingleton<IStartup>(new ServerStartup(SharedCtx, connectionString));
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