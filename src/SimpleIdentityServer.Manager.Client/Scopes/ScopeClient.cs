namespace SimpleAuth.Manager.Client.Scopes
{
    using System;
    using System.Threading.Tasks;
    using Configuration;
    using Results;
    using Shared;
    using Shared.Requests;
    using Shared.Responses;

    internal sealed class ScopeClient : IScopeClient
    {
        private readonly IAddScopeOperation _addScopeOperation;
        private readonly IDeleteScopeOperation _deleteScopeOperation;
        private readonly IGetAllScopesOperation _getAllScopesOperation;
        private readonly IGetScopeOperation _getScopeOperation;
        private readonly IUpdateScopeOperation _updateScopeOperation;
        private readonly IGetConfigurationOperation _configurationClient;
        private readonly ISearchScopesOperation _searchScopesOperation;

        public ScopeClient(IAddScopeOperation addScopeOperation, IDeleteScopeOperation deleteScopeOperation, IGetAllScopesOperation getAllScopesOperation, IGetScopeOperation getScopeOperation, 
            IUpdateScopeOperation updateScopeOperation, IGetConfigurationOperation configurationClient, ISearchScopesOperation searchScopesOperation)
        {
            _addScopeOperation = addScopeOperation;
            _deleteScopeOperation = deleteScopeOperation;
            _getAllScopesOperation = getAllScopesOperation;
            _getScopeOperation = getScopeOperation;
            _updateScopeOperation = updateScopeOperation;
            _configurationClient = configurationClient;
            _searchScopesOperation = searchScopesOperation;
        }

        public async Task<BaseResponse> ResolveAdd(Uri wellKnownConfigurationUri, ScopeResponse scope, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _addScopeOperation.ExecuteAsync(new Uri(configuration.Content.Scopes), scope, authorizationHeaderValue).ConfigureAwait(false);
        }

        public async Task<BaseResponse> ResolveUpdate(Uri wellKnownConfigurationUri, ScopeResponse client, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _updateScopeOperation.ExecuteAsync(new Uri(configuration.Content.Scopes), client, authorizationHeaderValue).ConfigureAwait(false);
        }

        public async Task<GetScopeResult> ResolveGet(Uri wellKnownConfigurationUri, string scopeId, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _getScopeOperation.ExecuteAsync(new Uri(configuration.Content.Scopes + "/" + scopeId), authorizationHeaderValue).ConfigureAwait(false);
        }

        public async Task<BaseResponse> ResolvedDelete(Uri wellKnownConfigurationUri, string scopeId, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _deleteScopeOperation.ExecuteAsync(new Uri(configuration.Content.Scopes + "/" + scopeId), authorizationHeaderValue).ConfigureAwait(false);
        }

        public Task<GetAllScopesResult> GetAll(Uri scopesUri, string authorizationHeaderValue = null)
        {
            return _getAllScopesOperation.ExecuteAsync(scopesUri, authorizationHeaderValue);
        }

        public async Task<GetAllScopesResult> ResolveGetAll(Uri wellKnownConfigurationUri, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await GetAll(new Uri(configuration.Content.Scopes), authorizationHeaderValue).ConfigureAwait(false);
        }

        public async Task<PagedResult<ScopeResponse>> ResolveSearch(Uri wellKnownConfigurationUri, SearchScopesRequest searchScopesParameter, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _searchScopesOperation.ExecuteAsync(new Uri(configuration.Content.Scopes + "/.search"), searchScopesParameter, authorizationHeaderValue).ConfigureAwait(false);
        }
    }
}
