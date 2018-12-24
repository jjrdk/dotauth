namespace SimpleIdentityServer.Core.Authenticate
{
    using SimpleAuth.Shared.Models;

    public interface IClientSecretBasicAuthentication
    {
        Client AuthenticateClient(AuthenticateInstruction instruction, Client client);
        string GetClientId(AuthenticateInstruction instruction);
    }
}