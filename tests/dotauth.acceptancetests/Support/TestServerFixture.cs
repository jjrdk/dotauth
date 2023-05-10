namespace DotAuth.AcceptanceTests.Support;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit.Abstractions;

public sealed class TestServerFixture : IDisposable
{
    private readonly SharedContext _sharedCtx;

    public TestServer Server { get; }

    public Func<HttpClient> Client { get; }

    public TestServerFixture(ITestOutputHelper outputHelper, params string[] urls)
    {
        _sharedCtx = SharedContext.Instance;
        var startup = new ServerStartup(_sharedCtx, outputHelper);
        Server = new TestServer(
            new WebHostBuilder().UseUrls(urls)
                .ConfigureServices(
                    services => { startup.ConfigureServices(services); })
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(ServerStartup).Assembly.FullName)
                .Configure(startup.Configure));
        Client = () =>
        {
            var c = Server.CreateClient();
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return c;
        };

        _sharedCtx.Client = Server.CreateClient();
        _sharedCtx.Handler = Server.CreateHandler();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Server.Dispose();
        Client?.Invoke()?.Dispose();
    }
}
