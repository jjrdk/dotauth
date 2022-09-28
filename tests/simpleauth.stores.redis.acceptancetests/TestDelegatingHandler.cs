namespace SimpleAuth.Stores.Redis.AcceptanceTests;

using System.Net.Http;

internal sealed class TestDelegatingHandler : DelegatingHandler
{
    public TestDelegatingHandler(HttpMessageHandler innerHandler) : base(innerHandler)
    {
    }
}