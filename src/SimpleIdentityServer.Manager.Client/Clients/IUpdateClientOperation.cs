namespace SimpleIdentityServer.Manager.Client.Clients
{
    using System;
    using System.Threading.Tasks;
    using Shared;
    using Shared.Models;
    using Shared.Requests;

    public interface IUpdateClientOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri clientsUri, Client client, string authorizationHeaderValue = null);
    }
}