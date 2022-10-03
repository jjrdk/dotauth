namespace DotAuth.Tests.Validators;

using System.Net.Http;

internal sealed class TestHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _client;

    public TestHttpClientFactory(HttpClient client = null)
    {
        _client = client ?? new HttpClient();
    }

    /// <inheritdoc />
    public HttpClient CreateClient(string name)
    {
        return _client;
    }
}