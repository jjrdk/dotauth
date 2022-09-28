namespace SimpleAuth.Tests.Authenticate;

using Shared.Models;
using SimpleAuth.Authenticate;
using System;
using Xunit;

public sealed class ClientSecretPostAuthenticationFixture
{
    [Fact]
    public void When_Trying_To_Authenticate_The_Client_And_BothParametersAre_Null_Then_Exception_Is_Thrown()
    {
        Assert.Throws<NullReferenceException>(() => ClientSecretPostAuthentication.AuthenticateClient(null, null));
    }

    [Fact]
    public void When_Trying_To_Authenticate_The_Client_And_OneParameter_Is_Null_Then_Exception_Is_Thrown()
    {
        var authenticateInstruction = new AuthenticateInstruction();

        Assert.Throws<NullReferenceException>(() => ClientSecretPostAuthentication.AuthenticateClient(authenticateInstruction, null));
    }

    [Fact]
    public void When_Trying_To_Authenticate_The_Client_And_ThereIsNoSharedSecret_Then_Null_Is_Returned()
    {
        var authenticateInstruction = new AuthenticateInstruction
        {
            ClientSecretFromAuthorizationHeader = "notCorrectClientSecret"
        };
        var firstClient = new Client
        {
            Secrets = null
        };
        var secondClient = new Client
        {
            Secrets = new []
            {
                new ClientSecret
                {
                    Type = ClientSecretTypes.X509Thumbprint
                }
            }
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
            Secrets = new []
            {
                new ClientSecret
                {
                    Type = ClientSecretTypes.SharedSecret,
                    Value = "secret"
                }
            }
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
            Secrets = new []
            {
                new ClientSecret
                {
                    Type = ClientSecretTypes.SharedSecret,
                    Value = clientSecret
                }
            }
        };

        var result = ClientSecretPostAuthentication.AuthenticateClient(authenticateInstruction, client);

        Assert.NotNull(result);
    }
}