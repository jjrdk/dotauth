namespace SimpleIdentityServer.Core.Api.Profile.Actions
{
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    public interface IGetResourceOwnerClaimsAction
    {
        Task<ResourceOwner> Execute(string externalSubject);
    }
}