namespace SimpleIdentityServer.Core.WebSite.User.Actions
{
    using System.Threading.Tasks;
    using Parameters;

    public interface IAddUserOperation
    {
        Task<bool> Execute(AddUserParameter addUserParameter, AuthenticationParameter authenticationParameter, string scimBaseUrl = null, bool addScimResource = false, string issuer = null);
    }
}