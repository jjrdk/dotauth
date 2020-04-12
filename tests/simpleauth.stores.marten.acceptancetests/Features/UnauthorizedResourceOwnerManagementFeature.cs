namespace SimpleAuth.Stores.Marten.AcceptanceTests.Features
{
    using System.Net;
    using SimpleAuth.Client;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;
    using Xbehave;
    using Xunit;

    public class UnauthorizedResourceOwnerManagementFeature : UnauthorizedManagementFeatureBase
    {
        [Scenario]
        public void RejectAddResourceOwner()
        {
            GenericResponse<AddResourceOwnerResponse> response = null;

            "When adding resource owner".x(
                async () =>
                {
                    response = await _managerClient.AddResourceOwner(
                            new AddResourceOwnerRequest { Password = "test", Subject = "test" },
                            _grantedToken.AccessToken)
                        .ConfigureAwait(false);
                });

            "Then response has error.".x(
                () =>
                {
                    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
                });
        }

        [Scenario]
        public void RejectUpdateResourceOwnerPassword()
        {
            GenericResponse<object> response = null;

            "When updating resource owner password".x(
                async () =>
                {
                    response = await _managerClient.UpdateResourceOwnerPassword(
                            new UpdateResourceOwnerPasswordRequest { Password = "blah", Subject = "administrator" },
                            _grantedToken.AccessToken)
                        .ConfigureAwait(false);
                });

            "Then response has error.".x(
                () =>
                {
                    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
                });
        }

        [Scenario]
        public void RejectUpdateResourceOwnerClaims()
        {
            GenericResponse<object> response = null;

            "When updating resource owner password".x(
                async () =>
                {
                    response = await _managerClient.UpdateResourceOwnerClaims(
                            new UpdateResourceOwnerClaimsRequest
                            {
                                Claims = new[] { new ClaimData { Type = "something", Value = "else" } },
                                Subject = "administrator"
                            },
                            _grantedToken.AccessToken)
                        .ConfigureAwait(false);
                });

            "Then response has error.".x(
                () =>
                {
                    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
                });
        }

        [Scenario]
        public void RejectDeleteResourceOwner()
        {
            GenericResponse<object> response = null;

            "When deleting resource owner".x(
                async () =>
                {
                    response = await _managerClient.DeleteResourceOwner(
                            "administrator",
                            _grantedToken.AccessToken)
                        .ConfigureAwait(false);
                });

            "Then response has error.".x(
                () =>
                {
                    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
                });
        }

        [Scenario]
        public void RejectedListResourceOwners()
        {
            GenericResponse<ResourceOwner[]> response = null;

            "When listing resource owners".x(
                async () =>
                {
                    response = await _managerClient.GetAllResourceOwners(_grantedToken.AccessToken)
                        .ConfigureAwait(false);
                });

            "Then response has error.".x(
                () =>
                {
                    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
                });
        }
    }
}