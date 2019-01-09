namespace SimpleAuth.Manager.Client.ResourceOwners
{
    using System;
    using System.Threading.Tasks;
    using Shared;
    using Shared.Requests;

    public interface IUpdateResourceOwnerClaimsOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri resourceOwnerUri, UpdateResourceOwnerClaimsRequest updateResourceOwnerClaimsRequest, string authorizationHeaderValue = null);
    }
}