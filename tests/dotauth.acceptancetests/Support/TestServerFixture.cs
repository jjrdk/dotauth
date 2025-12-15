namespace DotAuth.AcceptanceTests.Support;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

public sealed class TestServerFixture : IDisposable
{
    private readonly SharedContext _sharedCtx;

    public IHost Server { get; }

    public Func<HttpClient> Client { get; }

    public TestServerFixture(ITestOutputHelper outputHelper, params string[] urls)
    {
        _sharedCtx = SharedContext.Instance;
        var startup = new ServerStartup(_sharedCtx, outputHelper);
        Server = new HostBuilder().ConfigureWebHost(builder =>
        {
            builder.UseUrls(urls).UseTestServer()
                .ConfigureServices(services => { startup.ConfigureServices(services); })
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(ServerStartup).Assembly.FullName)
                .Configure(startup.Configure);
        }).Start();
        Client = () =>
        {
            var c = Server.GetTestClient();
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return c;
        };

        _sharedCtx.Client = Server.GetTestClient();
    }

    public void Dispose()
    {
        Client.Invoke().Dispose();
        Server.Dispose();
        GC.SuppressFinalize(this);
    }
}
