using System.Net.Http;

namespace SimpleAuth.Manager.Client.Scopes
{
    using Configuration;
    using Results;
    using Shared;
    using Shared.Requests;
    using Shared.Responses;
    using System;
    using System.Threading.Tasks;

    internal sealed class ScopeClient
    {
        private readonly AddScopeOperation _addScopeOperation;
        private readonly DeleteScopeOperation _deleteScopeOperation;
        private readonly GetAllScopesOperation _getAllScopesOperation;
        private readonly GetScopeOperation _getScopeOperation;
        private readonly UpdateScopeOperation _updateScopeOperation;
        private readonly GetConfigurationOperation _configurationClient;
        private readonly SearchScopesOperation _searchScopesOperation;

        public ScopeClient(HttpClient client)
        {
            _addScopeOperation = new AddScopeOperation(client);
            _deleteScopeOperation = new DeleteScopeOperation(client);
            _getAllScopesOperation = new GetAllScopesOperation(client);
            _getScopeOperation = new GetScopeOperation(client);
            _updateScopeOperation = new UpdateScopeOperation(client);
            _configurationClient = new GetConfigurationOperation(client);
            _searchScopesOperation = new SearchScopesOperation(client);
        }

        public async Task<BaseResponse> ResolveAdd(
            Uri wellKnownConfigurationUri,
            ScopeResponse scope,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _addScopeOperation.ExecuteAsync(
                    new Uri(configuration.Content.Scopes),
                    scope,
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<BaseResponse> ResolveUpdate(
            Uri wellKnownConfigurationUri,
            ScopeResponse client,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _updateScopeOperation.ExecuteAsync(
                    new Uri(configuration.Content.Scopes),
                    client,
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<GetScopeResult> ResolveGet(
            Uri wellKnownConfigurationUri,
            string scopeId,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _getScopeOperation.ExecuteAsync(
                    new Uri(configuration.Content.Scopes + "/" + scopeId),
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<BaseResponse> ResolvedDelete(
            Uri wellKnownConfigurationUri,
            string scopeId,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _deleteScopeOperation.ExecuteAsync(
                    new Uri(configuration.Content.Scopes + "/" + scopeId),
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public Task<GetAllScopesResult> GetAll(Uri scopesUri, string authorizationHeaderValue = null)
        {
            return _getAllScopesOperation.ExecuteAsync(scopesUri, authorizationHeaderValue);
        }

        public async Task<GetAllScopesResult> ResolveGetAll(
            Uri wellKnownConfigurationUri,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await GetAll(new Uri(configuration.Content.Scopes), authorizationHeaderValue).ConfigureAwait(false);
        }

        public async Task<PagedResult<ScopeResponse>> ResolveSearch(
            Uri wellKnownConfigurationUri,
            SearchScopesRequest searchScopesParameter,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _searchScopesOperation.ExecuteAsync(
                    new Uri(configuration.Content.Scopes + "/.search"),
                    searchScopesParameter,
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }
    }
}
