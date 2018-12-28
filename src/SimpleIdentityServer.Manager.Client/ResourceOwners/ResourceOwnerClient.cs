namespace SimpleAuth.Manager.Client.ResourceOwners
{
    using System;
    using System.Threading.Tasks;
    using Configuration;
    using Results;
    using Shared;
    using Shared.Requests;
    using Shared.Responses;

    internal sealed class ResourceOwnerClient : IResourceOwnerClient
    {
        private readonly IAddResourceOwnerOperation _addResourceOwnerOperation;
        private readonly IDeleteResourceOwnerOperation _deleteResourceOwnerOperation;
        private readonly IGetAllResourceOwnersOperation _getAllResourceOwnersOperation;
        private readonly IGetResourceOwnerOperation _getResourceOwnerOperation;
        private readonly IUpdateResourceOwnerPasswordOperation _updateResourceOwnerPasswordOperation;
        private readonly IUpdateResourceOwnerClaimsOperation _updateResourceOwnerClaimsOperation;
        private readonly IGetConfigurationOperation _configurationClient;
        private readonly ISearchResourceOwnersOperation _searchResourceOwnersOperation;

        public ResourceOwnerClient(IAddResourceOwnerOperation addResourceOwnerOperation,
            IDeleteResourceOwnerOperation deleteResourceOwnerOperation,
            IGetAllResourceOwnersOperation getAllResourceOwnersOperation,
            IGetResourceOwnerOperation getResourceOwnerOperation,
            IUpdateResourceOwnerClaimsOperation updateResourceOwnerClaimsOperation,
            IUpdateResourceOwnerPasswordOperation updateResourceOwnerPasswordOperation,
            IGetConfigurationOperation configurationClient,
            ISearchResourceOwnersOperation searchResourceOwnersOperation)
        {
            _addResourceOwnerOperation = addResourceOwnerOperation;
            _deleteResourceOwnerOperation = deleteResourceOwnerOperation;
            _getAllResourceOwnersOperation = getAllResourceOwnersOperation;
            _getResourceOwnerOperation = getResourceOwnerOperation;
            _updateResourceOwnerClaimsOperation = updateResourceOwnerClaimsOperation;
            _updateResourceOwnerPasswordOperation = updateResourceOwnerPasswordOperation;
            _configurationClient = configurationClient;
            _searchResourceOwnersOperation = searchResourceOwnersOperation;
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
