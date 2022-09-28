﻿namespace SimpleAuth.AcceptanceTests;

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

    public SharedContext SharedCtx { get; }

    public TestServerFixture(ITestOutputHelper outputHelper, params string[] urls)
    {
        SharedCtx = SharedContext.Instance;
        var startup = new ServerStartup(SharedCtx, outputHelper);
        Server = new TestServer(
            new WebHostBuilder().UseUrls(urls)
                .ConfigureServices(
                    services =>
                    {
                        startup.ConfigureServices(services);
                    })
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(ServerStartup).Assembly.FullName)
                .Configure(startup.Configure));
        Client = () =>
        {
            var c = Server.CreateClient();
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return c;
        };

        SharedCtx.Client = Server.CreateClient();
        SharedCtx.Handler = Server.CreateHandler();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Server.Dispose();
        Client?.Invoke()?.Dispose();
    }
}