namespace DotAuth.Stores.Marten.AcceptanceTests.Features;

using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public sealed class ResourceOwnerManagementFeature : AuthorizedManagementFeatureBase
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
                var response = await ManagerClient.AddResourceOwner(
                        new AddResourceOwnerRequest { Password = "test", Subject = "test" },
                        GrantedToken.AccessToken)
                    .ConfigureAwait(false) as Option<AddResourceOwnerResponse>.Result;

                Assert.NotNull(response);

                subject = response.Item.Subject;
            });

        "Then resource owner is local account".x(
            async () =>
            {
                var response = await ManagerClient.GetResourceOwner("test", GrantedToken.AccessToken)
                    .ConfigureAwait(false) as Option<ResourceOwner>.Result;

                Assert.NotNull(response);
                Assert.True(response.Item.IsLocalAccount);
            });
    }

    [Scenario]
    public void SuccessUpdateResourceOwnerPassword()
    {
        "When adding resource owner".x(
            async () =>
            {
                var response = await ManagerClient.AddResourceOwner(
                        new AddResourceOwnerRequest { Password = "test", Subject = "test" },
                        GrantedToken.AccessToken)
                    .ConfigureAwait(false) as Option<AddResourceOwnerResponse>.Result;

                Assert.NotNull(response);
            });

        "Then can update resource owner password".x(
            async () =>
            {
                var response = await ManagerClient.UpdateResourceOwnerPassword(
                        new UpdateResourceOwnerPasswordRequest { Subject = "test", Password = "test2" },
                        GrantedToken.AccessToken)
                    .ConfigureAwait(false);

                Assert.IsType<Option.Success>(response);
            });
    }
}