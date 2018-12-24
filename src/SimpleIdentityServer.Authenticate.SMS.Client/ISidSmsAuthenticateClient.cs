namespace SimpleIdentityServer.Authenticate.SMS.Client
{
    using System.Threading.Tasks;
    using Common.Requests;
    using SimpleAuth.Shared;

    public interface ISidSmsAuthenticateClient
    {
        Task<BaseResponse> Send(string requestUrl, ConfirmationCodeRequest request, string authorizationValue = null);
    }
}