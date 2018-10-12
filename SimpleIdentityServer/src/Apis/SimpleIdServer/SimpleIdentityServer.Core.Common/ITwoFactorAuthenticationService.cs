namespace SimpleIdentityServer.Core.Common
{
    using System.Threading.Tasks;
    using Models;

    public interface ITwoFactorAuthenticationService
    {
        Task SendAsync(string code, ResourceOwner user);
        string RequiredClaim { get; }
        string Name { get; }
    }
}
