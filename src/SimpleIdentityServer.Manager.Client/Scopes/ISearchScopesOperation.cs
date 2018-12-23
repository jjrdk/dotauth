namespace SimpleIdentityServer.Manager.Client.Scopes
{
    using System;
    using System.Threading.Tasks;
    using Results;
    using Shared.Requests;
    using Shared.Responses;

    public interface ISearchScopesOperation
    {
        Task<PagedResult<ScopeResponse>> ExecuteAsync(Uri scopesUri, SearchScopesRequest parameter, string authorizationHeaderValue = null);
    }
}