namespace SimpleIdentityServer.Core.Authenticate
{
    using Common.Models;

    public interface IClientSecretBasicAuthentication
    {
        Client AuthenticateClient(AuthenticateInstruction instruction, Client client);
        string GetClientId(AuthenticateInstruction instruction);
    }
}