namespace SimpleIdentityServer.Core
{
    using System;
    using System.Threading.Tasks;
    using Common.DTOs.Requests;

    public interface IJwksClient
    {
        Task<JsonWebKeySet> ResolveAsync(Uri configurationUrl);
    }
}