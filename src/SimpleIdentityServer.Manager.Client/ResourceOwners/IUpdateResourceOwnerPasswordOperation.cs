namespace SimpleAuth.Manager.Client.ResourceOwners
{
    using System;
    using System.Threading.Tasks;
    using Shared;
    using Shared.Requests;

    public interface IUpdateResourceOwnerPasswordOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri resourceOwnerUri, UpdateResourceOwnerPasswordRequest updateResourceOwnerPasswordRequest, string authorizationHeaderValue = null);
    }
}