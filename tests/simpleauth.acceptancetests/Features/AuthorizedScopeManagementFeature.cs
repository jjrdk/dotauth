namespace SimpleAuth.AcceptanceTests.Features;

using SimpleAuth.Shared;
using SimpleAuth.Shared.Models;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public sealed class AuthorizedScopeManagementFeature : AuthorizedManagementFeatureBase
{
    /// <inheritdoc />
    public AuthorizedScopeManagementFeature(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Scenario]
    public void SuccessScopeLoad()
    {
        Scope scope = null!;

        "When requesting existing scope".x(
            async () =>
            {
                var response = await _managerClient.GetScope("test", _administratorToken.AccessToken)
                    .ConfigureAwait(false) as Option<Scope>.Result;

                Assert.NotNull(response);

                scope = response!.Item;
            });

        "then scope information is returned".x(() => { Assert.Equal("test", scope.Name); });
    }
}