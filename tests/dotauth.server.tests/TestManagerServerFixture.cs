namespace DotAuth.Server.Tests;

using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

public sealed class TestManagerServerFixture : IDisposable
{
    public TestServer Server { get; }
    public Func<HttpClient> Client { get; }

    public TestManagerServerFixture()
    {
        var startup = new FakeManagerStartup();
        var host = new HostBuilder().ConfigureWebHost(builder =>
        {
            builder
                .UseTestServer()
                .UseUrls("http://localhost:5000")
                .ConfigureServices(startup.ConfigureServices)
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(FakeManagerStartup).Assembly.FullName)
                .Configure(startup.Configure);
        }).Build();
        host.Start();
        Server = host.GetTestServer();
        Client = Server.CreateClient;
    }

    public void Dispose()
    {
        Server.Dispose();
        Client().Dispose();
        GC.SuppressFinalize(this);
    }
}
