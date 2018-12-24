namespace SimpleIdentityServer.Manager.Client.Clients
{
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;

    public interface IDeleteClientOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri clientsUri, string authorizationHeaderValue = null);
    }
}