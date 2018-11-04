namespace SimpleIdentityServer.UserManagement.Client
{
    using System.Threading.Tasks;
    using Common.Requests;
    using Results;
    using Shared;

    public interface IProfileClient
    {
        Task<BaseResponse> LinkMyProfile(string requestUrl, LinkProfileRequest linkProfileRequest, string authorizationHeaderValue = null);
        Task<BaseResponse> LinkProfile(string requestUrl, string currentSubject, LinkProfileRequest linkProfileRequest, string authorizationHeaderValue = null);
        Task<BaseResponse> UnlinkMyProfile(string requestUrl, string externalSubject, string authorizationHeaderValue = null);
        Task<BaseResponse> UnlinkProfile(string requestUrl, string externalSubject, string currentSubject, string authorizationHeaderValue = null);
        Task<GetProfilesResult> GetMyProfiles(string requestUrl, string authorizationHeaderValue = null);
        Task<GetProfilesResult> GetProfiles(string requestUrl, string currentSubject, string authorizationHeaderValue = null);
    }
}