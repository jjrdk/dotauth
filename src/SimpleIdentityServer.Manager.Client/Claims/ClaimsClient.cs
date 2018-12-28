namespace SimpleAuth.Manager.Client.Claims
{
    using System;
    using System.Threading.Tasks;
    using Configuration;
    using Results;
    using Shared;
    using Shared.Requests;
    using Shared.Responses;

    internal sealed class ClaimsClient : IClaimsClient
    {
        private readonly IAddClaimOperation _addClaimOperation;
        private readonly IDeleteClaimOperation _deleteClaimOperation;
        private readonly IGetClaimOperation _getClaimOperation;
        private readonly ISearchClaimsOperation _searchClaimsOperation;
        private readonly IGetConfigurationOperation _configurationClient;
        private readonly IGetAllClaimsOperation _getAllClaimsOperation;

        public ClaimsClient(IAddClaimOperation addClaimOperation,
            IDeleteClaimOperation deleteClaimOperation,
            IGetClaimOperation getClaimOperation,
            ISearchClaimsOperation searchClaimsOperation,
            IGetConfigurationOperation configurationClient,
            IGetAllClaimsOperation getAllClaimsOperation)
        {
            _addClaimOperation = addClaimOperation;
            _deleteClaimOperation = deleteClaimOperation;
            _getClaimOperation = getClaimOperation;
            _searchClaimsOperation = searchClaimsOperation;
            _configurationClient = configurationClient;
            _getAllClaimsOperation = getAllClaimsOperation;
        }

        public async Task<BaseResponse> Add(
            Uri wellKnownConfigurationUri,
            ClaimResponse claim,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _addClaimOperation
                .ExecuteAsync(new Uri(configuration.Content.Claims), claim, authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<GetClaimResult> Get(Uri wellKnownConfigurationUri,
            string claimId,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _getClaimOperation
                .ExecuteAsync(new Uri(configuration.Content.Claims + "/" + claimId), authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<BaseResponse> Delete(Uri wellKnownConfigurationUri,
            string claimId,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _deleteClaimOperation
                .ExecuteAsync(new Uri(configuration.Content.Claims + "/" + claimId), authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<PagedResult<ClaimResponse>> Search(Uri wellKnownConfigurationUri,
            SearchClaimsRequest searchClaimsRequest,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _searchClaimsOperation.ExecuteAsync(new Uri(configuration.Content.Claims + "/.search"),
                    searchClaimsRequest,
                    authorizationHeaderValue)
                .ConfigureAwait(false);
        }

        public async Task<GetAllClaimsResult> GetAll(Uri wellKnownConfigurationUri,
            string authorizationHeaderValue = null)
        {
            var configuration =
                await _configurationClient.ExecuteAsync(wellKnownConfigurationUri).ConfigureAwait(false);
            return await _getAllClaimsOperation
                .ExecuteAsync(new Uri(configuration.Content.Claims), authorizationHeaderValue)
                .ConfigureAwait(false);
        }
    }
}
