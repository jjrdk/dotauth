namespace SimpleIdentityServer.Manager.Client.Claims
{
    using System;
    using System.Threading.Tasks;
    using Results;
    using Shared.Requests;
    using Shared.Responses;

    public interface ISearchClaimsOperation
    {
        Task<PagedResult<ClaimResponse>> ExecuteAsync(Uri claimsUri, SearchClaimsRequest parameter, string authorizationHeaderValue = null);
    }
}