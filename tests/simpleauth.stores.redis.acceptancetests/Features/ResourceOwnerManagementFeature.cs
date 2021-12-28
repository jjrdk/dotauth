namespace SimpleAuth.Stores.Redis.AcceptanceTests.Features
{
    using SimpleAuth.Client;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;
    using Xbehave;
    using Xunit;
    using Xunit.Abstractions;

    public class ResourceOwnerManagementFeature : AuthorizedManagementFeatureBase
    {
        /// <inheritdoc />
        public ResourceOwnerManagementFeature(ITestOutputHelper output)
            : base(output)
        {
        }

        [Scenario]
        public void SuccessAddResourceOwner()
        {
            string subject = null!;

            "When adding resource owner".x(
                async () =>
                {
                    var response = await _managerClient.AddResourceOwner(
                            new AddResourceOwnerRequest { Password = "test", Subject = "test" },
                            _grantedToken.AccessToken)
                        .ConfigureAwait(false) as Option<AddResourceOwnerResponse>.Result;

                    Assert.NotNull(response);

                    subject = response.Item.Subject;
                });

            "Then resource owner is local account".x(
                async () =>
                {
                    var response = await _managerClient.GetResourceOwner("test", _grantedToken.AccessToken)
                        .ConfigureAwait(false) as Option<ResourceOwner>.Result;

                    Assert.True(response.Item.IsLocalAccount);
                });
        }

        [Scenario]
        public void SuccessUpdateResourceOwnerPassword()
        {
            "When adding resource owner".x(
                async () =>
                {
                    var response = await _managerClient.AddResourceOwner(
                            new AddResourceOwnerRequest { Password = "test", Subject = "test" },
                            _grantedToken.AccessToken)
                        .ConfigureAwait(false);

                    Assert.IsType<Option<AddResourceOwnerResponse>.Result>(response);
                });

            "Then can update resource owner password".x(
                async () =>
                {
                    var response = await _managerClient.UpdateResourceOwnerPassword(
                            new UpdateResourceOwnerPasswordRequest { Subject = "test", Password = "test2" },
                            _grantedToken.AccessToken)
                        .ConfigureAwait(false);

                    Assert.IsType<Option.Success>(response);
                });
        }
    }
}
