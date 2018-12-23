namespace SimpleIdentityServer.Manager.Client.ResourceOwners
{
    using System;
    using System.Threading.Tasks;
    using Shared;
    using Shared.Requests;

    public interface IAddResourceOwnerOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri resourceOwnerUri, AddResourceOwnerRequest resourceOwner, string authorizationHeaderValue = null);
    }
}