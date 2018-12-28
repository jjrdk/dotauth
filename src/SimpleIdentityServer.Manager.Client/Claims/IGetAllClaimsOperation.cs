namespace SimpleAuth.Manager.Client.Claims
{
    using System;
    using System.Threading.Tasks;
    using Results;

    public interface IGetAllClaimsOperation
    {
        Task<GetAllClaimsResult> ExecuteAsync(Uri claimsUri, string authorizationHeaderValue = null);
    }
}