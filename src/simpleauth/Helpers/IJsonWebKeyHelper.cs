namespace SimpleAuth.Helpers
{
    using System;
    using System.Threading.Tasks;
    using Shared;

    public interface IJsonWebKeyHelper
    {
        Task<JsonWebKey> GetJsonWebKey(string kid, Uri uri);
    }
}