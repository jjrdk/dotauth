namespace SimpleIdentityServer.Uma.Core.Api.ConfigurationController.Actions
{
    using System.Threading.Tasks;
    using Responses;

    public interface IGetConfigurationAction
    {
        Task<ConfigurationResponse> Execute();
    }
}