namespace SimpleIdentityServer.Manager.Client.ResourceOwners
{
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Requests;

    public interface IUpdateResourceOwnerPasswordOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri resourceOwnerUri, UpdateResourceOwnerPasswordRequest updateResourceOwnerPasswordRequest, string authorizationHeaderValue = null);
    }
}