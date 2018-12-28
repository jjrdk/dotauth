namespace SimpleAuth.Uma.Client.Configuration
{
    using System;
    using System.Threading.Tasks;
    using Shared.DTOs;

    public interface IGetConfigurationOperation
    {
        Task<ConfigurationResponse> ExecuteAsync(Uri configurationUri);
    }
}