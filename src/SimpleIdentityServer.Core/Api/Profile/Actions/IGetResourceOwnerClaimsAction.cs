namespace SimpleAuth.Api.Profile.Actions
{
    using System.Threading.Tasks;
    using Shared.Models;

    public interface IGetResourceOwnerClaimsAction
    {
        Task<ResourceOwner> Execute(string externalSubject);
    }
}