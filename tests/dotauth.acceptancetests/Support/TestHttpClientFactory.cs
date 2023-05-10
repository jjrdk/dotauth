namespace DotAuth.AcceptanceTests.Support;

using System.Net.Http;

internal sealed class TestHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _client;

    public TestHttpClientFactory(HttpClient client)
    {
        _client = client;
    }

    /// <inheritdoc />
    public HttpClient CreateClient(string name)
    {
        return _client;
    }
}