namespace SimpleIdentityServer.Core.WebSite.User.Actions
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public interface IGetConsentsOperation
    {
        Task<IEnumerable<Common.Models.Consent>> Execute(ClaimsPrincipal claimsPrincipal);
    }
}