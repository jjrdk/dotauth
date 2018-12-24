namespace SimpleIdentityServer.Manager.Client.ResourceOwners
{
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Requests;

    public interface IUpdateResourceOwnerClaimsOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri resourceOwnerUri, UpdateResourceOwnerClaimsRequest updateResourceOwnerClaimsRequest, string authorizationHeaderValue = null);
    }
}