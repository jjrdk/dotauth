namespace SimpleIdentityServer.Uma.Core.Api.ConfigurationController
{
    using System.Threading.Tasks;
    using Responses;

    public interface IConfigurationActions
    {
        Task<ConfigurationResponse> GetConfiguration();
    }
}