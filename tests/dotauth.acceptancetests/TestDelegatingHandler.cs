﻿namespace DotAuth.AcceptanceTests;

using System.Net.Http;

internal sealed class TestDelegatingHandler : DelegatingHandler
{
    public TestDelegatingHandler(HttpMessageHandler innerHandler) : base(innerHandler)
    {
    }
}