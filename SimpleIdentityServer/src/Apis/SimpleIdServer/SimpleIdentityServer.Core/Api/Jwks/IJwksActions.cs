namespace SimpleIdentityServer.Core.Api.Jwks
{
    using System.Threading.Tasks;
    using Common.DTOs.Requests;

    public interface IJwksActions
    {
        Task<JsonWebKeySet> GetJwks();
        Task<bool> RotateJwks();
    }
}