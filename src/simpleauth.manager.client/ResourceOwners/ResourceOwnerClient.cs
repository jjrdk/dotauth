using System.Net.Http;

namespace SimpleAuth.Manager.Client.ResourceOwners
{
    using Configuration;
    using Results;
    using Shared;
    using Shared.Requests;
    using Shared.Responses;
    using System;
    using System.Threading.Tasks;

    internal sealed class ResourceOwnerClient
    {
        private readonly AddResourceOwnerOperation _addResourceOwnerOperation;
        private readonly DeleteResourceOwnerOperation _deleteResourceOwnerOperation;
        private readonly GetAllResourceOwnersOperation _getAllResourceOwnersOperation;
        private readonly GetResourceOwnerOperation _getResourceOwnerOperation;
        private readonly UpdateResourceOwnerPasswordOperation _updateResourceOwnerPasswordOperation;
        private readonly UpdateResourceOwnerClaimsOperation _updateResourceOwnerClaimsOperation;
        private readonly GetConfigurationOperation _configurationClient;
        private readonly SearchResourceOwnersOperation _searchResourceOwnersOperation;

        public ResourceOwnerClient(HttpClient client)
        {
            _addResourceOwnerOperation = new AddResourceOwnerOperation(client);
            _deleteResourceOwnerOperation = new DeleteResourceOwnerOperation(client);
            _getAllResourceOwnersOperation = new GetAllResourceOwnersOperation(client);
            _getResourceOwnerOperation = new GetResourceOwnerOperation(client);
            _updateResourceOwnerClaimsOperation = new UpdateResourceOwnerClaimsOperation(client);
            _updateResourceOwnerPasswordOperation = new UpdateResourceOwnerPasswordOperation(client);
            _configurationClient = new GetConfigurationOperation(client);
            _searchResourceOwnersOperation = new SearchResourceOwnersOperation(client);
        }

        public async Task<BaseResponse> ResolveAdd(Uri wellKnownConfigurationUri,
            AddResourceOwnerRequest request,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _addResourceOwnerOperation
                .ExecuteAsync(new Uri(configuration.Content.ResourceOwners), request, authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<BaseResponse> ResolveUpdateClaims(Uri wellKnownConfigurationUri,
            UpdateResourceOwnerClaimsRequest request,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _updateResourceOwnerClaimsOperation
                .ExecuteAsync(new Uri(configuration.Content.ResourceOwners + "/claims"),
                    request,
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<BaseResponse> ResolveUpdatePassword(Uri wellKnownConfigurationUri,
            UpdateResourceOwnerPasswordRequest request,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _updateResourceOwnerPasswordOperation
                .ExecuteAsync(new Uri(configuration.Content.ResourceOwners + "/password"),
                    request,
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<GetResourceOwnerResult> ResolveGet(Uri wellKnownConfigurationUri,
            string resourceOwnerId,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _getResourceOwnerOperation
                .ExecuteAsync(new Uri(configuration.Content.ResourceOwners + "/" + resourceOwnerId),
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<BaseResponse> ResolveDelete(Uri wellKnownConfigurationUri,
            string resourceOwnerId,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _deleteResourceOwnerOperation
                .ExecuteAsync(new Uri(configuration.Content.ResourceOwners + "/" + resourceOwnerId),
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<GetAllResourceOwnersResult> ResolveGetAll(Uri wellKnownConfigurationUri,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await GetAll(new Uri(configuration.Content.ResourceOwners), authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public Task<GetAllResourceOwnersResult> GetAll(Uri resourceOwnerUri, string authorizationHeaderValue = null)
        {
            return _getAllResourceOwnersOperation.ExecuteAsync(resourceOwnerUri, authorizationHeaderValue);
        }

        public async Task<PagedResult<ResourceOwnerResponse>> ResolveSearch(Uri wellKnownConfigurationUri,
            SearchResourceOwnersRequest searchResourceOwnersRequest,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _searchResourceOwnersOperation
                .ExecuteAsync(new Uri(configuration.Content.ResourceOwners + "/.search"),
                    searchResourceOwnersRequest,
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }
    }
}
