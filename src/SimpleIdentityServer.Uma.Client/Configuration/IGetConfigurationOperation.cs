namespace SimpleIdentityServer.Uma.Client.Configuration
{
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Uma.Shared.DTOs;

    public interface IGetConfigurationOperation
    {
        Task<ConfigurationResponse> ExecuteAsync(Uri configurationUri);
    }
}