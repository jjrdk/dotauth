namespace DotAuth.Stores.Marten.AcceptanceTests.Features;

using System.Net;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public sealed class UnauthorizedScopeManagementFeature : UnauthorizedManagementFeatureBase
{
    /// <inheritdoc />
    public UnauthorizedScopeManagementFeature(ITestOutputHelper output)
        : base(output)
    {
    }

    [Scenario]
    public void RejectedScopeLoad()
    {
        Option<Scope>.Error scope = null!;

        "When requesting existing scope".x(
            async () =>
            {
                scope = await ManagerClient.GetScope("test", GrantedToken.AccessToken)
                    .ConfigureAwait(false) as Option<Scope>.Error;
            });

        "then error is returned".x(() => { Assert.Equal(HttpStatusCode.Forbidden, scope.Details.Status); });
    }

    [Scenario]
    public void RejectedAddScope()
    {
        Option<Scope>.Error scope = null!;

        "When adding new scope".x(
            async () =>
            {
                scope = await ManagerClient.AddScope(
                        new Scope {Name = "test", Claims = new[] {"openid"}},
                        GrantedToken.AccessToken)
                    .ConfigureAwait(false) as Option<Scope>.Error;
            });

        "then error is returned".x(() => { Assert.Equal(HttpStatusCode.Forbidden, scope.Details.Status); });
    }
}