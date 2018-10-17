namespace SimpleIdentityServer.Core.Api.UserInfo
{
    using System.Threading.Tasks;
    using Results;

    public interface IUserInfoActions
    {
        Task<UserInfoResult> GetUserInformation(string accessToken);
    }
}