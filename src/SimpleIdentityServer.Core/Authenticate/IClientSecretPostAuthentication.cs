namespace SimpleIdentityServer.Core.Authenticate
{
    using Common.Models;

    public interface IClientSecretPostAuthentication
    {
        Client AuthenticateClient(AuthenticateInstruction instruction, Client client);
        string GetClientId(AuthenticateInstruction instruction);
    }
}