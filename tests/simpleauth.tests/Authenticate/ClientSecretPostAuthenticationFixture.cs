namespace SimpleAuth.Tests.Authenticate
{
    using Shared.Models;
    using SimpleAuth.Authenticate;
    using System;
    using System.Collections.Generic;
    using Xunit;

    public sealed class ClientSecretPostAuthenticationFixture
    {
        private ClientSecretPostAuthentication _clientSecretPostAuthentication;

        [Fact]
        public void When_Trying_To_Authenticate_The_Client_And_OneParameter_Is_Null_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var authenticateInstruction = new AuthenticateInstruction();

            Assert.Throws<ArgumentNullException>(() => _clientSecretPostAuthentication.AuthenticateClient(null, null));
            Assert.Throws<ArgumentNullException>(() => _clientSecretPostAuthentication.AuthenticateClient(authenticateInstruction, null));
        }

        [Fact]
        public void When_Trying_To_Authenticate_The_Client_And_ThereIsNoSharedSecret_Then_Null_Is_Returned()
        {
            InitializeFakeObjects();
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
                Secrets = new List<ClientSecret>
                {
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.X509Thumbprint
                    }
                }
            };


            Assert.Null(_clientSecretPostAuthentication.AuthenticateClient(authenticateInstruction, firstClient));
            Assert.Null(_clientSecretPostAuthentication.AuthenticateClient(authenticateInstruction, secondClient));
        }

        [Fact]
        public void When_Trying_To_Authenticate_The_Client_And_Credentials_Are_Not_Correct_Then_Null_Is_Returned()
        {
            InitializeFakeObjects();
            var authenticateInstruction = new AuthenticateInstruction
            {
                ClientSecretFromHttpRequestBody = "notCorrectClientSecret"
            };
            var client = new Client
            {
                Secrets = new List<ClientSecret>
                {
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.SharedSecret,
                        Value = "secret"
                    }
                }
            };

            var result = _clientSecretPostAuthentication.AuthenticateClient(authenticateInstruction, client);

            Assert.Null(result);
        }

        [Fact]
        public void When_Trying_To_Authenticate_The_Client_And_Credentials_Are_Correct_Then_Client_Is_Returned()
        {
            InitializeFakeObjects();
            const string clientSecret = "clientSecret";
            var authenticateInstruction = new AuthenticateInstruction
            {
                ClientSecretFromHttpRequestBody = clientSecret
            };
            var client = new Client
            {
                Secrets = new List<ClientSecret>
                {
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.SharedSecret,
                        Value = clientSecret
                    }
                }
            };

            var result = _clientSecretPostAuthentication.AuthenticateClient(authenticateInstruction, client);

            Assert.NotNull(result);
        }

        [Fact]
        public void When_Requesting_ClientId_And_Instruction_Is_Null_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            Assert.Throws<ArgumentNullException>(() => _clientSecretPostAuthentication.GetClientId(null));
        }

        [Fact]
        public void When_Requesting_ClientId_Then_ClientId_Is_Returned()
        {
            InitializeFakeObjects();
            const string clientId = "clientId";
            var instruction = new AuthenticateInstruction
            {
                ClientIdFromHttpRequestBody = clientId
            };

            var result = _clientSecretPostAuthentication.GetClientId(instruction);

            Assert.True(clientId == result);

        }

        private void InitializeFakeObjects()
        {
            _clientSecretPostAuthentication = new ClientSecretPostAuthentication();
        }
    }
}
