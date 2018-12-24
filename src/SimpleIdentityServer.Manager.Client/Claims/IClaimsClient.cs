namespace SimpleIdentityServer.Manager.Client.Claims
{
    using System;
    using System.Threading.Tasks;
    using Results;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    public interface IClaimsClient
    {
        Task<BaseResponse> Add(Uri wellKnownConfigurationUri,
            ClaimResponse claim,
            string authorizationHeaderValue = null);

        Task<GetClaimResult> Get(Uri wellKnownConfigurationUri, string claimId, string authorizationHeaderValue = null);

        Task<BaseResponse> Delete(Uri wellKnownConfigurationUri,
            string claimId,
            string authorizationHeaderValue = null);

        Task<PagedResult<ClaimResponse>> Search(Uri wellKnownConfigurationUri,
            SearchClaimsRequest searchClaimsRequest,
            string authorizationHeaderValue = null);

        Task<GetAllClaimsResult> GetAll(Uri wellKnownConfigurationUri, string authorizationHeaderValue = null);
    }
}