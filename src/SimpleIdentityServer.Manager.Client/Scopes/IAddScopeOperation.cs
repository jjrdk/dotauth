namespace SimpleIdentityServer.Manager.Client.Scopes
{
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;

    public interface IAddScopeOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri scopesUri, ScopeResponse scope, string authorizationHeaderValue = null);
    }
}