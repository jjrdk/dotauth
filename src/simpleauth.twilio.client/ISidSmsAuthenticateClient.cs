namespace SimpleAuth.Twilio.Client
{
    using System.Threading.Tasks;
    using Shared.Requests;
    using SimpleAuth.Shared;

    public interface ISidSmsAuthenticateClient
    {
        Task<BaseResponse> Send(string requestUrl, ConfirmationCodeRequest request, string authorizationValue = null);
    }
}