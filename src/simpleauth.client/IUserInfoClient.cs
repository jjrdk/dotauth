namespace SimpleIdentityServer.Client
{
    using System.Threading.Tasks;
    using Results;

    public interface IUserInfoClient
    {
        Task<GetUserInfoResult> Resolve(string configurationUrl, string accessToken, bool inBody = false);
    }
}