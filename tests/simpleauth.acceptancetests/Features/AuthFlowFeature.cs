﻿namespace SimpleAuth.AcceptanceTests.Features;

using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public abstract class AuthFlowFeature
{
    private readonly ITestOutputHelper _outputHelper;
    protected const string WellKnownOpenidConfiguration = "https://localhost/.well-known/openid-configuration";
    protected const string BaseUrl = "http://localhost:5000";
    protected TestServerFixture _fixture = null!;
    protected JsonWebKeySet _jwks = null!;

    public AuthFlowFeature(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        IdentityModelEventSource.ShowPII = true;
    }

    [Background]
    public void Background()
    {
        "Given a running auth server".x(() => _fixture = new TestServerFixture(_outputHelper, BaseUrl))
            .Teardown(() => _fixture?.Dispose());

        "And the server signing keys".x(
            async () =>
            {
                var keysJson = await _fixture.Client().GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);
                var keys = JsonConvert.DeserializeObject<JsonWebKeySet>(keysJson);

                _jwks = keys!;
                Assert.NotEmpty(_jwks?.Keys);
            });
    }
}