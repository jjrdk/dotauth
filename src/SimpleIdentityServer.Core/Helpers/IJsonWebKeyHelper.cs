namespace SimpleIdentityServer.Core.Helpers
{
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;

    public interface IJsonWebKeyHelper
    {
        Task<JsonWebKey> GetJsonWebKey(string kid, Uri uri);
    }
}