namespace SimpleIdentityServer.Manager.Client.Scopes
{
    using System;
    using System.Threading.Tasks;
    using Results;
    using Shared;
    using Shared.Requests;
    using Shared.Responses;

    public interface IScopeClient
    {
        Task<BaseResponse> ResolveAdd(Uri wellKnownConfigurationUri, ScopeResponse scope, string authorizationHeaderValue = null);
        Task<BaseResponse> ResolveUpdate(Uri wellKnownConfigurationUri, ScopeResponse client, string authorizationHeaderValue = null);
        Task<GetScopeResult> ResolveGet(Uri wellKnownConfigurationUri, string scopeId, string authorizationHeaderValue = null);
        Task<BaseResponse> ResolvedDelete(Uri wellKnownConfigurationUri, string scopeId, string authorizationHeaderValue = null);
        Task<GetAllScopesResult> GetAll(Uri scopesUri, string authorizationHeaderValue = null);
        Task<GetAllScopesResult> ResolveGetAll(Uri wellKnownConfigurationUri, string authorizationHeaderValue = null);
        Task<PagedResult<ScopeResponse>> ResolveSearch(Uri wellKnownConfigurationUri, SearchScopesRequest searchScopesParameter, string authorizationHeaderValue = null);
    }
}