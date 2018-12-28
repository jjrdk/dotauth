namespace SimpleAuth.Manager.Client.Scopes
{
    using System;
    using System.Threading.Tasks;
    using Shared;

    public interface IDeleteScopeOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri scopesUri, string authorizationHeaderValue = null);
    }
}