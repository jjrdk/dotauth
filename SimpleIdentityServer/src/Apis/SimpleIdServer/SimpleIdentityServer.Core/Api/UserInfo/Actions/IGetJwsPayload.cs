namespace SimpleIdentityServer.Core.Api.UserInfo.Actions
{
    using System.Threading.Tasks;
    using Results;

    public interface IGetJwsPayload
    {
        Task<UserInfoResult> Execute(string accessToken);
    }
}