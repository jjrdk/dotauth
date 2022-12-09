namespace DotAuth.AcceptanceTests.Features;

using System;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Responses;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public abstract class AuthorizedManagementFeatureBase
{
    protected readonly ITestOutputHelper OutputHelper;
    protected const string BaseUrl = "http://localhost";
    protected static readonly Uri WellKnownUmaConfiguration = new(BaseUrl + "/.well-known/openid-configuration");
    protected TestServerFixture Fixture = null!;
    protected ManagementClient _managerClient = null!;
    protected TokenClient _tokenClient = null!;
    protected GrantedTokenResponse _administratorToken = null!;

    public AuthorizedManagementFeatureBase(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
    }

    [Background]
    public void Background()
    {
        "Given a running auth server".x(() => Fixture = new TestServerFixture(OutputHelper, BaseUrl))
            .Teardown(() => Fixture.Dispose());

        "and a manager client".x(
            async () =>
            {
                _managerClient = await ManagementClient.Create(Fixture.Client, WellKnownUmaConfiguration)
                    .ConfigureAwait(false);
            });

        "and a token client".x(
            () =>
            {
                _tokenClient = new TokenClient(
                    TokenCredentials.FromClientCredentials("manager_client", "manager_client"),
                    Fixture.Client,
                    WellKnownUmaConfiguration);
            });

        "and a manager token".x(
            async () =>
            {
                var result = await _tokenClient.GetToken(
                        TokenRequest.FromPassword("administrator", "password", new[] {"manager", "offline"}))
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;

                Assert.NotNull(result.Item);

                _administratorToken = result.Item;
            });
    }
}