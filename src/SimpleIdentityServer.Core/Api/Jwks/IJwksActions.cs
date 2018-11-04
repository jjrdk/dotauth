namespace SimpleIdentityServer.Core.Api.Jwks
{
    using System.Threading.Tasks;
    using Shared.Requests;

    public interface IJwksActions
    {
        Task<JsonWebKeySet> GetJwks();
        Task<bool> RotateJwks();
    }
}