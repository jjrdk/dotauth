namespace SimpleIdentityServer.Manager.Client.Claims
{
    using System;
    using System.Threading.Tasks;
    using Shared;
    using Shared.Responses;

    public interface IAddClaimOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri claimsUri, ClaimResponse claim, string authorizationHeaderValue = null);
    }
}