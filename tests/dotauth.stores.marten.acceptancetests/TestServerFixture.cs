namespace DotAuth.Stores.Marten.AcceptanceTests;

using System;
using System.Net.Http;
using DotAuth.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

public sealed class TestServerFixture : IDisposable
{
    public TestServer Server { get; }
    public Func<HttpClient> Client { get; }
    public SharedContext SharedCtx { get; }

    public TestServerFixture(ITestOutputHelper outputHelper, string connectionString, params string[] urls)
    {
        Globals.ApplicationName = "test";
        SharedCtx = SharedContext.Instance;
        var startup = new ServerStartup(SharedCtx, connectionString, outputHelper);
        var host = new HostBuilder().ConfigureWebHost(builder =>
        {
            builder
                .UseTestServer()
                .UseUrls(urls)
                .UseConfiguration(
                    new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false).AddEnvironmentVariables()
                        .Build())
                .ConfigureServices(services =>
                {
                    services.AddSingleton(SharedCtx);
                    startup.ConfigureServices(services);
                })
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(ServerStartup).Assembly.FullName)
                .Configure(startup.Configure);
        }).Build();
        host.Start();
        Server = host.GetTestServer();
        Client = () => Server.CreateClient();
        SharedCtx.Client = Client;
        SharedCtx.Handler = () => Server.CreateHandler();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Server.Dispose();
        Client?.Invoke()?.Dispose();
    }
}
