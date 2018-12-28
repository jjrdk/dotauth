namespace SimpleAuth.Manager.Client.Clients
{
    using System;
    using System.Threading.Tasks;
    using Results;
    using Shared.Requests;
    using Shared.Responses;

    public interface ISearchClientOperation
    {
        Task<PagedResult<ClientResponse>> ExecuteAsync(Uri clientsUri, SearchClientsRequest parameter, string authorizationHeaderValue = null);
    }
}