namespace SimpleAuth.Manager.Client
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

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

        public async Task<GenericResponse<object>> ResolveAdd(Uri wellKnownConfigurationUri,
            AddResourceOwnerRequest request,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.Execute(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _addResourceOwnerOperation
                .Execute(new Uri(configuration.Content.ResourceOwners), request, authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<GenericResponse<object>> ResolveUpdateClaims(Uri wellKnownConfigurationUri,
            UpdateResourceOwnerClaimsRequest request,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.Execute(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _updateResourceOwnerClaimsOperation
                .Execute(new Uri(configuration.Content.ResourceOwners + "/claims"),
                    request,
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<GenericResponse<object>> ResolveUpdatePassword(Uri wellKnownConfigurationUri,
            UpdateResourceOwnerPasswordRequest request,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.Execute(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _updateResourceOwnerPasswordOperation
                .Execute(new Uri(configuration.Content.ResourceOwners + "/password"),
                    request,
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<GenericResponse<ResourceOwnerResponse>> ResolveGet(Uri wellKnownConfigurationUri,
            string resourceOwnerId,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.Execute(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _getResourceOwnerOperation
                .Execute(new Uri(configuration.Content.ResourceOwners + "/" + resourceOwnerId),
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<GenericResponse<object>> ResolveDelete(Uri wellKnownConfigurationUri,
            string resourceOwnerId,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.Execute(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _deleteResourceOwnerOperation
                .Execute(new Uri(configuration.Content.ResourceOwners + "/" + resourceOwnerId),
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<GenericResponse<ResourceOwnerResponse[]>> ResolveGetAll(Uri wellKnownConfigurationUri,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.Execute(wellKnownConfigurationUri).ConfigureAwait(false);
            return await GetAll(new Uri(configuration.Content.ResourceOwners), authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public Task<GenericResponse<ResourceOwnerResponse[]>> GetAll(Uri resourceOwnerUri, string authorizationHeaderValue = null)
        {
            return _getAllResourceOwnersOperation.Execute(resourceOwnerUri, authorizationHeaderValue);
        }

        public async Task<GenericResponse<PagedResponse<ResourceOwnerResponse>>> ResolveSearch(Uri wellKnownConfigurationUri,
            SearchResourceOwnersRequest searchResourceOwnersRequest,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.Execute(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _searchResourceOwnersOperation
                .Execute(new Uri(configuration.Content.ResourceOwners + "/.search"),
                    searchResourceOwnersRequest,
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }
    }
}
