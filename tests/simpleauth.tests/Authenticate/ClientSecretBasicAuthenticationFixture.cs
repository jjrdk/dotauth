namespace SimpleAuth.Tests.Authenticate
{
    using Shared.Models;
    using SimpleAuth.Authenticate;
    using System;
    using System.Collections.Generic;
    using Xunit;

    public sealed class ClientSecretBasicAuthenticationFixture
    {
        [Fact]
        public void When_Trying_To_Authenticate_The_Client_And_OneParameter_Is_Null_Then_Exception_Is_Thrown()
        {
            var authenticateInstruction = new AuthenticateInstruction();

            Assert.Throws<ArgumentNullException>(() => ClientSecretBasicAuthentication.AuthenticateClient(null, null));
            Assert.Throws<ArgumentNullException>(() => authenticateInstruction.AuthenticateClient(null));
        }

        [Fact]
        public void When_Trying_To_Authenticate_The_Client_And_ThereIsNoSharedSecret_Then_Null_Is_Returned()
        {
            var authenticateInstruction = new AuthenticateInstruction
            {
                ClientSecretFromAuthorizationHeader = "notCorrectClientSecret"
            };
            var firstClient = new Client {Secrets = null};
            var secondClient = new Client
            {
                Secrets = new[] {new ClientSecret {Type = ClientSecretTypes.X509Thumbprint}}
            };

            Assert.Null(authenticateInstruction.AuthenticateClient(firstClient));
            Assert.Null(authenticateInstruction.AuthenticateClient(secondClient));
        }

        [Fact]
        public void When_Trying_To_Authenticate_The_Client_And_Credentials_Are_Not_Correct_Then_Null_Is_Returned()
        {
            var authenticateInstruction = new AuthenticateInstruction
            {
                ClientSecretFromAuthorizationHeader = "notCorrectClientSecret"
            };
            var client = new Client
            {
                Secrets = new[] {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "not_correct"}}
            };

            var result = authenticateInstruction.AuthenticateClient(client);

            Assert.Null(result);
        }

        [Fact]
        public void When_Trying_To_Authenticate_The_Client_And_Credentials_Are_Correct_Then_Client_Is_Returned()
        {
            const string clientSecret = "clientSecret";
            var authenticateInstruction = new AuthenticateInstruction
            {
                ClientSecretFromAuthorizationHeader = clientSecret
            };
            var client = new Client
            {
                Secrets = new[] {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = clientSecret}}
            };

            var result = authenticateInstruction.AuthenticateClient(client);

            Assert.NotNull(result);
        }
    }
}
