namespace SimpleAuth.Manager.Client.ResourceOwners
{
    using System;
    using System.Threading.Tasks;
    using Results;
    using Shared;
    using Shared.Requests;
    using Shared.Responses;

    public interface IResourceOwnerClient
    {
        Task<BaseResponse> ResolveAdd(Uri wellKnownConfigurationUri, AddResourceOwnerRequest request, string authorizationHeaderValue = null);
        Task<BaseResponse> ResolveUpdateClaims(Uri wellKnownConfigurationUri, UpdateResourceOwnerClaimsRequest resourceOwner, string authorizationHeaderValue = null);
        Task<BaseResponse> ResolveUpdatePassword(Uri wellKnownConfigurationUri, UpdateResourceOwnerPasswordRequest request, string authorizationHeaderValue = null);
        Task<GetResourceOwnerResult> ResolveGet(Uri wellKnownConfigurationUri, string resourceOwnerId, string authorizationHeaderValue = null);
        Task<BaseResponse> ResolveDelete(Uri wellKnownConfigurationUri, string resourceOwnerId, string authorizationHeaderValue = null);
        Task<GetAllResourceOwnersResult> GetAll(Uri resourceOwnerUri, string authorizationHeaderValue = null);
        Task<GetAllResourceOwnersResult> ResolveGetAll(Uri wellKnownConfigurationUri, string authorizationHeaderValue = null);
        Task<PagedResult<ResourceOwnerResponse>> ResolveSearch(Uri wellKnownConfigurationUri, SearchResourceOwnersRequest searchResourceOwnersRequest, string authorizationHeaderValue = null);
    }
}