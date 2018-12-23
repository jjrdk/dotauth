namespace SimpleIdentityServer.Manager.Client.Clients
{
    using System;
    using System.Threading.Tasks;
    using Results;

    public interface IGetClientOperation
    {
        Task<GetClientResult> ExecuteAsync(Uri clientsUri, string authorizationHeaderValue = null);
    }
}