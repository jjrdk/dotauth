namespace SimpleAuth.Manager.Client.ResourceOwners
{
    using System;
    using System.Threading.Tasks;
    using Results;
    using Shared.Requests;
    using Shared.Responses;

    public interface ISearchResourceOwnersOperation
    {
        Task<PagedResult<ResourceOwnerResponse>> ExecuteAsync(Uri resourceOwnerUri, SearchResourceOwnersRequest parameter, string authorizationHeaderValue = null);
    }
}