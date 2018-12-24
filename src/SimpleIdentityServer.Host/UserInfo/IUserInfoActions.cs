namespace SimpleIdentityServer.Host.UserInfo
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    public interface IUserInfoActions
    {
        Task<IActionResult> GetUserInformation(string accessToken);
    }
}