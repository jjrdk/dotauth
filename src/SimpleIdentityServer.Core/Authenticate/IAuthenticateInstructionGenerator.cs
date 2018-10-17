namespace SimpleIdentityServer.Core.Authenticate
{
    using System.Net.Http.Headers;

    public interface IAuthenticateInstructionGenerator
    {
        AuthenticateInstruction GetAuthenticateInstruction(AuthenticationHeaderValue authenticationHeaderValue);
    }
}