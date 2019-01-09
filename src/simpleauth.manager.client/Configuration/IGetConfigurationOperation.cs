namespace SimpleAuth.Manager.Client.Configuration
{
    using System;
    using System.Threading.Tasks;
    using Results;

    public interface IGetConfigurationOperation
    {
        Task<GetConfigurationResult> ExecuteAsync(Uri wellKnownConfigurationUrl);
    }
}