namespace SimpleIdentityServer.Core.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common;
    using Common.Models;

    public interface ITwoFactorAuthenticationHandler
    {
        IEnumerable<ITwoFactorAuthenticationService> GetAll();
        ITwoFactorAuthenticationService Get(string twoFactorAuthType);
        Task<bool> SendCode(string code, string twoFactorAuthType, ResourceOwner user);
    }
}