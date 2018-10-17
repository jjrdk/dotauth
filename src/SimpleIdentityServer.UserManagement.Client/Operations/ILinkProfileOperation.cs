namespace SimpleIdentityServer.UserManagement.Client.Operations
{
    using System.Threading.Tasks;
    using Core.Common;
    using Requests;

    public interface ILinkProfileOperation
    {
        Task<BaseResponse> Execute(string requestUrl, LinkProfileRequest linkProfileRequest, string authorizationHeaderValue = null);
        Task<BaseResponse> Execute(string requestUrl, string currentSubject, LinkProfileRequest linkProfileRequest, string authorizationHeaderValue = null);
    }
}