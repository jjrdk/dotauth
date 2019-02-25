namespace SimpleAuth.AcceptanceTests.Features
{
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using Xbehave;
    using Xunit;

    public class UnauthorizedScopeManagementFeature : UnauthorizedManagementFeatureBase
    {
        [Scenario]
        public void RejectedScopeLoad()
        {
            GenericResponse<Scope> scope = null;

            "When requesting existing scope".x(
                async () =>
                {
                    scope = await _managerClient.GetScope("test", _grantedToken.AccessToken)
                        .ConfigureAwait(false);
                });

            "then error is returned".x(() => { Assert.True(scope.ContainsError); });
        }
    }
}