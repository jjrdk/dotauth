namespace SimpleIdentityServer.Manager.Client.Clients
{
    using System;
    using System.Threading.Tasks;
    using Results;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    public interface IOpenIdClients
    {
        Task<AddClientResult> ResolveAdd(Uri wellKnownConfigurationUri, Client client, string authorizationHeaderValue = null);
        Task<BaseResponse> ResolveUpdate(Uri wellKnownConfigurationUri, Client client, string authorizationHeaderValue = null);
        Task<GetClientResult> ResolveGet(Uri wellKnownConfigurationUri, string clientId, string authorizationHeaderValue = null);
        Task<BaseResponse> ResolveDelete(Uri wellKnownConfigurationUri, string clientId, string authorizationHeaderValue = null);
        Task<GetAllClientResult> GetAll(Uri clientsUri, string authorizationHeaderValue = null);
        Task<GetAllClientResult> ResolveGetAll(Uri wellKnownConfigurationUri, string authorizationHeaderValue = null);
        Task<PagedResult<ClientResponse>> ResolveSearch(Uri wellKnownConfigurationUri, SearchClientsRequest searchClientParameter, string authorizationHeaderValue = null);
    }
}