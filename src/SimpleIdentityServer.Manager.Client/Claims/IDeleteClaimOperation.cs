namespace SimpleIdentityServer.Manager.Client.Claims
{
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;

    public interface IDeleteClaimOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri claimsUri, string authorizationHeaderValue = null);
    }
}