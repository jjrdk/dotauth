namespace SimpleAuth.Server.Tests
{
    using System;
    using System.Net.Http;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;

    public class TestManagerServerFixture : IDisposable
    {
        public TestServer Server { get; }
        public Func<HttpClient> Client { get; }

        public TestManagerServerFixture()
        {
            var startup = new FakeManagerStartup();
            Server = new TestServer(new WebHostBuilder()
                .UseUrls("http://localhost:5000")
                .ConfigureServices(startup.ConfigureServices)
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(FakeManagerStartup).Assembly.FullName)
                .Configure(startup.Configure));
            Client = Server.CreateClient;
        }

        public void Dispose()
        {
            Server.Dispose();
            Client?.Invoke()?.Dispose();
        }
    }
}
