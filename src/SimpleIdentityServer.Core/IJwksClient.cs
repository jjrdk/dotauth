namespace SimpleAuth
{
    using System;
    using System.Threading.Tasks;
    using Shared.Requests;

    public interface IJwksClient
    {
        Task<JsonWebKeySet> ResolveAsync(Uri configurationUrl);
    }
}