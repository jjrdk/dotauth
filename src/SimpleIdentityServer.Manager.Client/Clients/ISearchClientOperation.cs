namespace SimpleIdentityServer.Manager.Client.Clients
{
    using System;
    using System.Threading.Tasks;
    using Results;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    public interface ISearchClientOperation
    {
        Task<PagedResult<ClientResponse>> ExecuteAsync(Uri clientsUri, SearchClientsRequest parameter, string authorizationHeaderValue = null);
    }
}