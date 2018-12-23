namespace SimpleIdentityServer.Manager.Client.Scopes
{
    using System;
    using System.Threading.Tasks;
    using Shared;
    using Shared.Responses;

    public interface IUpdateScopeOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri scopesUri, ScopeResponse scope, string authorizationHeaderValue = null);
    }
}