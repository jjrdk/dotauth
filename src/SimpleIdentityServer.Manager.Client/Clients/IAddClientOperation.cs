namespace SimpleIdentityServer.Manager.Client.Clients
{
    using System;
    using System.Threading.Tasks;
    using Results;
    using SimpleAuth.Shared.Models;

    public interface IAddClientOperation
    {
        Task<AddClientResult> ExecuteAsync(Uri clientsUri, Client client, string authorizationHeaderValue = null);
    }
}