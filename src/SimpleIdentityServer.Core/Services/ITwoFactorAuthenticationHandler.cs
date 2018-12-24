namespace SimpleIdentityServer.Core.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;

    public interface ITwoFactorAuthenticationHandler
    {
        IEnumerable<ITwoFactorAuthenticationService> GetAll();
        ITwoFactorAuthenticationService Get(string twoFactorAuthType);
        Task<bool> SendCode(string code, string twoFactorAuthType, ResourceOwner user);
    }
}