namespace SimpleAuth.AcceptanceTests
{
    using System.Net.Http;

    internal class TestHttpClientFactory : IHttpClientFactory
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
}