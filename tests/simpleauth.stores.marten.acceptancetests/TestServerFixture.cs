namespace SimpleAuth.Stores.Marten.AcceptanceTests;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using SimpleAuth.Extensions;
using SimpleAuth.Shared;
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
        Server = new TestServer(
            new WebHostBuilder().UseUrls(urls)
                .UseConfiguration(
                    new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false).AddEnvironmentVariables().Build())
                .ConfigureServices(
                    services =>
                    {
                        services.AddSingleton(SharedCtx);
                        startup.ConfigureServices(services);
                    })
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(ServerStartup).Assembly.FullName)
                .Configure(startup.Configure));
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