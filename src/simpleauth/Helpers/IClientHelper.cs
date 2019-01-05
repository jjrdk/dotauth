namespace SimpleAuth.Helpers
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;
    using Shared.Models;

    public interface IClientHelper
    {
        Task<string> GenerateIdTokenAsync(string clientId, JwtPayload jwsPayload);
        Task<string> GenerateIdTokenAsync(Client client, JwtPayload jwsPayload);
    }
}