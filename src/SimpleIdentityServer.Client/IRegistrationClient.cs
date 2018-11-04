namespace SimpleIdentityServer.Client
{
    using System.Threading.Tasks;
    using Results;
    using Shared.Requests;

    public interface IRegistrationClient
    {
        Task<GetRegisterClientResult> ResolveAsync(ClientRequest client, string configurationUrl, string accessToken);
    }
}