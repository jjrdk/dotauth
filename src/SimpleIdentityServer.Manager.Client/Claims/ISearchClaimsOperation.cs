namespace SimpleIdentityServer.Manager.Client.Claims
{
    using System;
    using System.Threading.Tasks;
    using Results;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    public interface ISearchClaimsOperation
    {
        Task<PagedResult<ClaimResponse>> ExecuteAsync(Uri claimsUri, SearchClaimsRequest parameter, string authorizationHeaderValue = null);
    }
}