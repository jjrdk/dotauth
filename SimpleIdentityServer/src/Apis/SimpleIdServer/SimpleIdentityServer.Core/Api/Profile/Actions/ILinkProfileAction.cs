namespace SimpleIdentityServer.Core.Api.Profile.Actions
{
    using System.Threading.Tasks;

    public interface ILinkProfileAction
    {
        Task<bool> Execute(string localSubject, string externalSubject, string issuer, bool force = false);
    }
}