namespace SimpleIdentityServer.Core
{
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Requests;

    public interface IJwksClient
    {
        Task<JsonWebKeySet> ResolveAsync(Uri configurationUrl);
    }
}