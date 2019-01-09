namespace SimpleAuth.Manager.Client.Scopes
{
    using System;
    using System.Threading.Tasks;
    using Shared;
    using Shared.Responses;

    public interface IAddScopeOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri scopesUri, ScopeResponse scope, string authorizationHeaderValue = null);
    }
}