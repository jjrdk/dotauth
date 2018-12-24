namespace SimpleIdentityServer.Manager.Client.ResourceOwners
{
    using System;
    using System.Threading.Tasks;
    using Results;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    public interface ISearchResourceOwnersOperation
    {
        Task<PagedResult<ResourceOwnerResponse>> ExecuteAsync(Uri resourceOwnerUri, SearchResourceOwnersRequest parameter, string authorizationHeaderValue = null);
    }
}