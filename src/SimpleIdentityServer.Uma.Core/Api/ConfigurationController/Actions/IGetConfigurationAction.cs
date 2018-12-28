namespace SimpleAuth.Uma.Api.ConfigurationController.Actions
{
    using System.Threading.Tasks;
    using Responses;

    public interface IGetConfigurationAction
    {
        Task<ConfigurationResponse> Execute();
    }
}