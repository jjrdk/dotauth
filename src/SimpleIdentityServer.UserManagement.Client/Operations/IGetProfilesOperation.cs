namespace SimpleIdentityServer.UserManagement.Client.Operations
{
    using System.Threading.Tasks;
    using Results;

    public interface IGetProfilesOperation
    {
        Task<GetProfilesResult> Execute(string requestUrl, string currentSubject, string authorizationHeaderValue = null);
        Task<GetProfilesResult> Execute(string requestUrl, string authorizationHeaderValue = null);
    }
}