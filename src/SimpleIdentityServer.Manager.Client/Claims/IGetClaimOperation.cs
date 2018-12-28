namespace SimpleAuth.Manager.Client.Claims
{
    using System;
    using System.Threading.Tasks;
    using Results;

    public interface IGetClaimOperation
    {
        Task<GetClaimResult> ExecuteAsync(Uri claimsUri, string authorizationHeaderValue = null);
    }
}