namespace SimpleIdentityServer.Manager.Client.Scopes
{
    using System;
    using System.Threading.Tasks;
    using Results;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    public interface ISearchScopesOperation
    {
        Task<PagedResult<ScopeResponse>> ExecuteAsync(Uri scopesUri, SearchScopesRequest parameter, string authorizationHeaderValue = null);
    }
}