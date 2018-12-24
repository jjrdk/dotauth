namespace SimpleIdentityServer.Core.WebSite.User.Actions
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    public interface IGetUserOperation
    {
        Task<ResourceOwner> Execute(ClaimsPrincipal claimsPrincipal);
    }
}