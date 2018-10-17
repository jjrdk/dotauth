namespace SimpleIdentityServer.Core.Authenticate
{
    using Common.Models;

    public interface IClientTlsAuthentication
    {
        Client AuthenticateClient(AuthenticateInstruction instruction, Client client);
    }
}