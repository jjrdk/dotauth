namespace SimpleAuth.Authenticate
{
    using System.Threading.Tasks;

    public interface IClientAssertionAuthentication
    {
        string GetClientId(AuthenticateInstruction instruction);
        Task<AuthenticationResult> AuthenticateClientWithPrivateKeyJwtAsync(AuthenticateInstruction instruction, string issuer);
        Task<AuthenticationResult> AuthenticateClientWithClientSecretJwtAsync(AuthenticateInstruction instruction, string clientSecret, string issuer);
    }
}