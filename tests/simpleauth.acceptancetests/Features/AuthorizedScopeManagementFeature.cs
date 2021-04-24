﻿namespace SimpleAuth.AcceptanceTests.Features
{
    using SimpleAuth.Shared.Models;
    using Xbehave;
    using Xunit;
    using Xunit.Abstractions;

    public class AuthorizedScopeManagementFeature : AuthorizedManagementFeatureBase
    {
        /// <inheritdoc />
        public AuthorizedScopeManagementFeature(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Scenario]
        public void SuccessScopeLoad()
        {
            Scope scope = null;

            "When requesting existing scope".x(
                async () =>
                {
                    var response = await _managerClient.GetScope("test", _administratorToken.AccessToken)
                        .ConfigureAwait(false);

                    Assert.False(response.HasError);

                    scope = response.Content;
                });

            "then scope information is returned".x(() => { Assert.Equal("test", scope.Name); });
        }
    }
}
