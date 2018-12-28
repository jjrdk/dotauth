namespace SimpleIdentityServer.Client
{
    using System.Threading.Tasks;
    using Results;
    using SimpleAuth.Shared.Models;

    public interface IRegistrationClient
    {
        Task<GetRegisterClientResult> ResolveAsync(Client client, string configurationUrl, string accessToken);
    }
}