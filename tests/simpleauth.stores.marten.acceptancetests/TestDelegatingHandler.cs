namespace SimpleAuth.Stores.Marten.AcceptanceTests
{
    using System.Net.Http;

    internal class TestDelegatingHandler : DelegatingHandler
    {
        public TestDelegatingHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }
    }
}