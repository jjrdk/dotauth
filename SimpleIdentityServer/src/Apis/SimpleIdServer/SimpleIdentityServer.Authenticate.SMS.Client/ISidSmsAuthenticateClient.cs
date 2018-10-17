namespace SimpleIdentityServer.Authenticate.SMS.Client
{
    using System.Threading.Tasks;
    using Common.Requests;
    using Core.Common;

    public interface ISidSmsAuthenticateClient
    {
        Task<BaseResponse> Send(string requestUrl, ConfirmationCodeRequest request, string authorizationValue = null);
    }
}