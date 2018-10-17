namespace SimpleIdentityServer.Core.Authenticate
{
    using System.Threading.Tasks;

    public interface IAuthenticateClient
    {
        Task<AuthenticationResult> AuthenticateAsync(AuthenticateInstruction instruction, string issuerName);
    }
}