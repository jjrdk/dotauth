namespace SimpleAuth.Manager.Client.Scopes
{
    using System;
    using System.Threading.Tasks;
    using Results;

    public interface IGetScopeOperation
    {
        Task<GetScopeResult> ExecuteAsync(Uri scopesUri, string authorizationHeaderValue = null);
    }
}