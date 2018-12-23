namespace SimpleIdentityServer.Manager.Client.Clients
{
    using System;
    using System.Threading.Tasks;
    using Shared;

    public interface IDeleteClientOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri clientsUri, string authorizationHeaderValue = null);
    }
}