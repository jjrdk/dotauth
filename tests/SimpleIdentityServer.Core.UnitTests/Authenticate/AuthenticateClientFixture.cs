using Moq;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Errors;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Authenticate
{
    using Logging;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    public sealed class AuthenticateClientFixture
    {
        private Mock<IClientSecretBasicAuthentication> _clientSecretBasicAuthenticationFake;
        private Mock<IClientSecretPostAuthentication> _clientSecretPostAuthenticationFake;
        private Mock<IClientAssertionAuthentication> _clientAssertionAuthenticationFake;
        private Mock<IClientTlsAuthentication> _clientTlsAuthenticationStub;
        private Mock<IClientStore> _clientRepositoryStub;
        private Mock<IOAuthEventSource> _oauthEventSource;
        private IAuthenticateClient _authenticateClient;

        [Fact]
        public async Task When_Passing_No_Authentication_Instruction_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _authenticateClient.AuthenticateAsync(null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_The_ClientId_Cannot_Be_Fetch_Then_Message_Error_Is_Returned_And_Result_Is_Null()
        {            InitializeFakeObjects();
            var authenticationInstruction = new AuthenticateInstruction();
            _clientAssertionAuthenticationFake.Setup(c => c.GetClientId(It.IsAny<AuthenticateInstruction>()))
                .Returns(string.Empty);

                        var result = await _authenticateClient.AuthenticateAsync(authenticationInstruction, null).ConfigureAwait(false);

                        Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheClientDoesntExist);
        }

        [Fact]
        public async Task When_The_ClientId_Is_Not_Valid_Then_Message_Error_Is_Returned_And_Result_Is_Null()
        {            InitializeFakeObjects();
            var authenticationInstruction = new AuthenticateInstruction();
            _clientAssertionAuthenticationFake.Setup(c => c.GetClientId(It.IsAny<AuthenticateInstruction>()))
                .Returns("clientId");
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(() => Task.FromResult((Client)null));

                        var result = await _authenticateClient.AuthenticateAsync(authenticationInstruction, null).ConfigureAwait(false);

                        Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheClientDoesntExist);
        }

        [Fact]
        public async Task When_Trying_To_Authenticate_The_Client_Via_Secret_Basic_Then_Operation_Is_Called_Client_Is_Returned_And_Events_Are_Logged()
        {            InitializeFakeObjects();
            const string clientId = "clientId";
            var authenticationInstruction = new AuthenticateInstruction();
            var client = new Client
            {
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_basic,
                ClientId = clientId
            };

            _clientAssertionAuthenticationFake.Setup(c => c.GetClientId(It.IsAny<AuthenticateInstruction>()))
                .Returns(clientId);
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _clientSecretBasicAuthenticationFake.Setup(
                c => c.AuthenticateClient(It.IsAny<AuthenticateInstruction>(), It.IsAny<Client>()))
                .Returns(client);

                        var result = await _authenticateClient.AuthenticateAsync(authenticationInstruction, null).ConfigureAwait(false);

                        Assert.NotNull(result.Client);
            _oauthEventSource.Verify(s => s.StartToAuthenticateTheClient(clientId, "client_secret_basic"));
            _oauthEventSource.Verify(s => s.FinishToAuthenticateTheClient(clientId, "client_secret_basic"));
        }

        [Fact]
        public async Task When_Trying_To_Authenticate_The_Client_Via_Secret_Basic_But_Operation_Failed_Then_Event_Is_Not_Logged_And_Null_Is_Returned()
        {            InitializeFakeObjects();
            const string clientId = "clientId";
            var authenticationInstruction = new AuthenticateInstruction();
            var client = new Client
            {
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_basic,
                ClientId = clientId
            };

            _clientAssertionAuthenticationFake.Setup(c => c.GetClientId(It.IsAny<AuthenticateInstruction>()))
                .Returns(clientId);
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _clientSecretBasicAuthenticationFake.Setup(
                c => c.AuthenticateClient(It.IsAny<AuthenticateInstruction>(), It.IsAny<Client>()))
                .Returns(() => null);

                        var result = await _authenticateClient.AuthenticateAsync(authenticationInstruction, null).ConfigureAwait(false);
            
                        Assert.Null(result.Client);
            _oauthEventSource.Verify(s => s.StartToAuthenticateTheClient(clientId, "client_secret_basic"));
            _oauthEventSource.Verify(s => s.FinishToAuthenticateTheClient(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        private void InitializeFakeObjects()
        {
            _clientSecretBasicAuthenticationFake = new Mock<IClientSecretBasicAuthentication>();
            _clientSecretPostAuthenticationFake = new Mock<IClientSecretPostAuthentication>();
            _clientAssertionAuthenticationFake = new Mock<IClientAssertionAuthentication>();
            _clientTlsAuthenticationStub = new Mock<IClientTlsAuthentication>();
            _clientRepositoryStub = new Mock<IClientStore>();
            _oauthEventSource = new Mock<IOAuthEventSource>();
            _authenticateClient = new AuthenticateClient(
                _clientSecretBasicAuthenticationFake.Object,
                _clientSecretPostAuthenticationFake.Object,
                _clientAssertionAuthenticationFake.Object,
                _clientTlsAuthenticationStub.Object,
                _clientRepositoryStub.Object,
                _oauthEventSource.Object);
        }
    }
}
