namespace SimpleAuth.Manager.Client.Scopes
{
    using System;
    using System.Threading.Tasks;
    using Results;

    public interface IGetAllScopesOperation
    {
        Task<GetAllScopesResult> ExecuteAsync(Uri scopesUri, string authorizationHeaderValue = null);
    }
}