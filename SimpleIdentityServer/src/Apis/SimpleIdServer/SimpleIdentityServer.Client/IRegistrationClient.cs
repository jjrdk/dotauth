namespace SimpleIdentityServer.Client
{
    using System.Threading.Tasks;
    using Results;

    public interface IRegistrationClient
    {
        Task<GetRegisterClientResult> ResolveAsync(Core.Common.DTOs.Requests.ClientRequest client, string configurationUrl, string accessToken);
    }
}