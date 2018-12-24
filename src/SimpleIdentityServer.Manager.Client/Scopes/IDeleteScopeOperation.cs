namespace SimpleIdentityServer.Manager.Client.Scopes
{
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;

    public interface IDeleteScopeOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri scopesUri, string authorizationHeaderValue = null);
    }
}