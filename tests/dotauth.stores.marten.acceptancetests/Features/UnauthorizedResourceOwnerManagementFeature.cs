﻿namespace DotAuth.Stores.Marten.AcceptanceTests.Features;

using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using System.Net;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public sealed class UnauthorizedResourceOwnerManagementFeature : UnauthorizedManagementFeatureBase
{
    /// <inheritdoc />
    public UnauthorizedResourceOwnerManagementFeature(ITestOutputHelper output)
        : base(output)
    {
    }

    [Scenario]
    public void RejectAddResourceOwner()
    {
        Option<AddResourceOwnerResponse>.Error response = null!;

        "When adding resource owner".x(
            async () =>
            {
                response = await ManagerClient.AddResourceOwner(
                        new AddResourceOwnerRequest { Password = "test", Subject = "test" },
                        GrantedToken.AccessToken)
                    .ConfigureAwait(false) as Option<AddResourceOwnerResponse>.Error;
            });

        "Then response has error.".x(
            () =>
            {
                Assert.Equal(HttpStatusCode.Forbidden, response.Details.Status);
            });
    }

    [Scenario]
    public void RejectUpdateResourceOwnerPassword()
    {
        Option.Error response = null!;

        "When updating resource owner password".x(
            async () =>
            {
                response = await ManagerClient.UpdateResourceOwnerPassword(
                        new UpdateResourceOwnerPasswordRequest { Password = "blah", Subject = "administrator" },
                        GrantedToken.AccessToken)
                    .ConfigureAwait(false) as Option.Error;
            });

        "Then response has error.".x(
            () =>
            {
                Assert.Equal(HttpStatusCode.Forbidden, response.Details.Status);
            });
    }

    [Scenario]
    public void RejectUpdateResourceOwnerClaims()
    {
        Option.Error response = null!;

        "When updating resource owner password".x(
            async () =>
            {
                response = await ManagerClient.UpdateResourceOwnerClaims(
                        new UpdateResourceOwnerClaimsRequest
                        {
                            Claims = new[] { new ClaimData { Type = "something", Value = "else" } },
                            Subject = "administrator"
                        },
                        GrantedToken.AccessToken)
                    .ConfigureAwait(false) as Option.Error;
            });

        "Then response has error.".x(
            () =>
            {
                Assert.Equal(HttpStatusCode.Forbidden, response.Details.Status);
            });
    }

    [Scenario]
    public void RejectDeleteResourceOwner()
    {
        Option.Error response = null!;

        "When deleting resource owner".x(
            async () =>
            {
                response = (await ManagerClient.DeleteResourceOwner(
                        "administrator",
                        GrantedToken.AccessToken)
                    .ConfigureAwait(false) as Option.Error)!;
            });

        "Then response has error.".x(
            () =>
            {
                Assert.Equal(HttpStatusCode.Forbidden, response.Details.Status);
            });
    }

    [Scenario]
    public void RejectedListResourceOwners()
    {
        Option<ResourceOwner[]>.Error response = null!;

        "When listing resource owners".x(
            async () =>
            {
                response = await ManagerClient.GetAllResourceOwners(GrantedToken.AccessToken)
                    .ConfigureAwait(false) as Option<ResourceOwner[]>.Error;
            });

        "Then response has error.".x(
            () =>
            {
                Assert.Equal(HttpStatusCode.Forbidden, response.Details.Status);
            });
    }
}