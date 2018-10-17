namespace SimpleIdentityServer.Core.Api.Jwks.Actions
{
    using System.Threading.Tasks;

    public interface IRotateJsonWebKeysOperation
    {
        Task<bool> Execute();
    }
}