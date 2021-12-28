namespace SimpleAuth.Stores.Marten.AcceptanceTests.Features
{
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using Xbehave;
    using Xunit;
    using Xunit.Abstractions;

    public class AuthorizedScopeManagementFeature : AuthorizedManagementFeatureBase
    {
        /// <inheritdoc />
        public AuthorizedScopeManagementFeature(ITestOutputHelper output)
            : base(output)
        {
        }

        [Scenario]
        public void SuccessScopeLoad()
        {
            Scope scope = null!;

            "When requesting existing scope".x(
                async () =>
                {
                    var response = await ManagerClient.GetScope("test", GrantedToken.AccessToken)
                        .ConfigureAwait(false) as Option<Scope>.Result;

                    scope = response!.Item;

                    Assert.NotNull(scope);
                });

            "then scope information is returned".x(() => { Assert.Equal("test", scope.Name); });
        }
    }
}
