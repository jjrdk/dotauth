namespace SimpleIdentityServer.Shared
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public interface IAccountFilter
    {
        Task<AccountFilterResult> Check(IEnumerable<Claim> claims);
    }
}
