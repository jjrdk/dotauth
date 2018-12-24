namespace SimpleIdentityServer.Core.Authenticate
{
    using SimpleAuth.Shared.Models;

    public interface IClientTlsAuthentication
    {
        Client AuthenticateClient(AuthenticateInstruction instruction, Client client);
    }
}