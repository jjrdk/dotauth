namespace SimpleIdentityServer.Manager.Client.Clients
{
    using System;
    using System.Threading.Tasks;
    using Results;
    using Shared.Models;
    using Shared.Requests;

    public interface IAddClientOperation
    {
        Task<AddClientResult> ExecuteAsync(Uri clientsUri, Client client, string authorizationHeaderValue = null);
    }
}