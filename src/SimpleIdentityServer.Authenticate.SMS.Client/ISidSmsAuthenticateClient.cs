namespace SimpleIdentityServer.Authenticate.SMS.Client
{
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Twilio.Shared.Requests;

    public interface ISidSmsAuthenticateClient
    {
        Task<BaseResponse> Send(string requestUrl, ConfirmationCodeRequest request, string authorizationValue = null);
    }
}