namespace SimpleIdentityServer.Core.WebSite.User.Actions
{
    using System.Threading.Tasks;

    public interface IUpdateUserTwoFactorAuthenticatorOperation
    {
        Task<bool> Execute(string subject, string twoFactorAuth);
    }
}