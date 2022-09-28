namespace SimpleAuth.AcceptanceTests.Features;

using System;
using SimpleAuth.Client;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Responses;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public abstract class UnauthorizedManagementFeatureBase
{
    private readonly ITestOutputHelper _outputHelper;
    private const string BaseUrl = "http://localhost";
    private static readonly Uri WellKnownUmaConfiguration = new(BaseUrl + "/.well-known/openid-configuration");
    protected TestServerFixture _fixture = null!;
    protected ManagementClient _managerClient = null!;
    protected TokenClient _tokenClient = null!;
    protected GrantedTokenResponse _grantedToken = null!;

    public UnauthorizedManagementFeatureBase(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Background]
    public void Background()
    {
        "Given a running auth server".x(() => _fixture = new TestServerFixture(_outputHelper, BaseUrl))
            .Teardown(() => _fixture.Dispose());

        "and a manager client".x(
            async () =>
            {
                _managerClient = await ManagementClient.Create(_fixture.Client, WellKnownUmaConfiguration)
                    .ConfigureAwait(false);
            });

        "and a token client".x(
            () =>
            {
                _tokenClient = new TokenClient(
                    TokenCredentials.FromClientCredentials("admin_client", "admin_client"),
                    _fixture.Client,
                    WellKnownUmaConfiguration);
            });

        "and an admin token".x(
            async () =>
            {
                var result = await _tokenClient.GetToken(TokenRequest.FromScopes("admin")).ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;

                Assert.NotNull(result.Item);

                _grantedToken = result.Item;
            });
    }
}