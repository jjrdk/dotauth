namespace SimpleAuth.Authenticate
{
    using Shared.Models;

    public interface IClientSecretBasicAuthentication
    {
        Client AuthenticateClient(AuthenticateInstruction instruction, Client client);
        string GetClientId(AuthenticateInstruction instruction);
    }
}