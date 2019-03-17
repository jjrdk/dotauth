namespace SimpleAuth.Stores.Marten.AcceptanceTests
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Net.Http;
    using Microsoft.Extensions.Configuration;

    public class TestServerFixture : IDisposable
    {
        public TestServer Server { get; }
        public HttpClient Client { get; }
        public SharedContext SharedCtx { get; }

        public TestServerFixture(params string[] urls)
        {
            SharedCtx = SharedContext.Instance;
            //var startup = new ServerStartup(SharedCtx, connectionString);
            Server = new TestServer(
                new WebHostBuilder()
                    .UseUrls(urls)
                    .ConfigureServices(
                        services =>
                        {
                            var configuration = new ConfigurationBuilder().AddUserSecrets<ServerStartup>()
                                .AddEnvironmentVariables()
                                .Build();
                            services.AddSingleton<IConfigurationRoot>(configuration);
                            services.AddSingleton<IConfiguration>(configuration);
                            services.AddSingleton(SharedCtx);
                            //services.AddSingleton<IStartup>(startup);
                        })
                    .UseSetting(WebHostDefaults.ApplicationKey, typeof(ServerStartup).Assembly.FullName)
                    .UseStartup<ServerStartup>());
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