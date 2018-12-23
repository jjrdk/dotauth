namespace SimpleIdentityServer.Manager.Client.ResourceOwners
{
    using System;
    using System.Threading.Tasks;
    using Results;

    public interface IGetAllResourceOwnersOperation
    {
        Task<GetAllResourceOwnersResult> ExecuteAsync(Uri resourceOwnerUri, string authorizationHeaderValue = null);
    }
}