namespace DotAuth.Tests.Authenticate;

using DotAuth.Authenticate;
using DotAuth.Shared.Models;
using Xunit;

public sealed class ClientSecretPostAuthenticationFixture
{
    [Fact]
    public void When_Trying_To_Authenticate_The_Client_And_ThereIsNoSharedSecret_Then_Null_Is_Returned()
    {
        var authenticateInstruction = new AuthenticateInstruction
        {
            ClientSecretFromAuthorizationHeader = "notCorrectClientSecret"
        };
        var firstClient = new Client
        {
            Secrets = []
        };
        var secondClient = new Client
        {
            Secrets =
            [
                new ClientSecret
                {
                    Type = ClientSecretTypes.X509Thumbprint
                }
            ]
        };


        Assert.Null(ClientSecretPostAuthentication.AuthenticateClient(authenticateInstruction, firstClient));
        Assert.Null(ClientSecretPostAuthentication.AuthenticateClient(authenticateInstruction, secondClient));
    }

    [Fact]
    public void When_Trying_To_Authenticate_The_Client_And_Credentials_Are_Not_Correct_Then_Null_Is_Returned()
    {
        var authenticateInstruction = new AuthenticateInstruction
        {
            ClientSecretFromHttpRequestBody = "notCorrectClientSecret"
        };
        var client = new Client
        {
            Secrets =
            [
                new ClientSecret
                {
                    Type = ClientSecretTypes.SharedSecret,
                    Value = "secret"
                }
            ]
        };

        var result = ClientSecretPostAuthentication.AuthenticateClient(authenticateInstruction, client);

        Assert.Null(result);
    }

    [Fact]
    public void When_Trying_To_Authenticate_The_Client_And_Credentials_Are_Correct_Then_Client_Is_Returned()
    {
        const string clientSecret = "clientSecret";
        var authenticateInstruction = new AuthenticateInstruction
        {
            ClientSecretFromHttpRequestBody = clientSecret
        };
        var client = new Client
        {
            Secrets =
            [
                new ClientSecret
                {
                    Type = ClientSecretTypes.SharedSecret,
                    Value = clientSecret
                }
            ]
        };

        var result = ClientSecretPostAuthentication.AuthenticateClient(authenticateInstruction, client);

        Assert.NotNull(result);
    }
}
