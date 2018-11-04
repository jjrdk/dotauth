namespace SimpleIdentityServer.Core.WebSite.User.Actions
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Shared.Models;

    public interface IGetConsentsOperation
    {
        Task<IEnumerable<Consent>> Execute(ClaimsPrincipal claimsPrincipal);
    }
}