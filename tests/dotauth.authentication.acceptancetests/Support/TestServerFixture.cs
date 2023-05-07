namespace DotAuth.Authentication.AcceptanceTests.Support;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit.Abstractions;

public sealed class TestServerFixture : IDisposable
{
    public TestServer Server { get; }

    public Func<HttpClient> Client { get; }

    public TestServerFixture(SharedContext sharedCtx, ITestOutputHelper outputHelper, params string[] urls)
    {
        var startup = new ServerStartup(sharedCtx, outputHelper);
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
    }

    public void Dispose()
    {
        Server.Dispose();
        Client.Invoke().Dispose();
    }
}
