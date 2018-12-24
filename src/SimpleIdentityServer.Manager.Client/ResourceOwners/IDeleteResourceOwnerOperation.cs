namespace SimpleIdentityServer.Manager.Client.ResourceOwners
{
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;

    public interface IDeleteResourceOwnerOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri resourceOwnerUri, string authorizationHeaderValue = null);
    }
}