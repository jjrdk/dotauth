namespace SimpleAuth.Tests.Api.Authorization
{
    using Exceptions;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Repositories;
    using SimpleAuth.Api.Authorization;
    using System;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Properties;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared.Errors;
    using Xunit;
    using Client = Shared.Models.Client;

    public sealed class GetAuthorizationCodeOperationFixture
    {
        private const string HttpsLocalhost = "https://localhost";
        private readonly GetAuthorizationCodeOperation _getAuthorizationCodeOperation;

        public GetAuthorizationCodeOperationFixture()
        {
            _getAuthorizationCodeOperation = new GetAuthorizationCodeOperation(
                new Mock<IAuthorizationCodeStore>().Object,
                new Mock<ITokenStore>().Object,
                new Mock<IScopeRepository>().Object,
                new Mock<IClientStore>().Object,
                new Mock<IConsentRepository>().Object,
                new InMemoryJwksRepository(),
                new NoOpPublisher());
        }

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            await Assert.ThrowsAsync<NullReferenceException>(
                    () => _getAuthorizationCodeOperation.Execute(null, null, null, null, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_The_Client_Grant_Type_Is_Not_Supported_Then_Exception_Is_Thrown()
        {
            const string clientId = "clientId";
            const string scope = "scope";
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseType = ResponseTypeNames.Code,
                RedirectUrl = new Uri(HttpsLocalhost),
                ClientId = clientId,
                Scope = scope,
                Claims = new ClaimsParameter()
            };

            var client = new Client
            {
                GrantTypes = new[] { GrantTypes.ClientCredentials },
                AllowedScopes = new[] { scope },
                RedirectionUrls = new[] { new Uri(HttpsLocalhost), }
            };
            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _getAuthorizationCodeOperation.Execute(
                        authorizationParameter,
                        new ClaimsPrincipal(),
                        client,
                        "",
                        CancellationToken.None))
                .ConfigureAwait(false);

            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InvalidRequest, exception.Code);
            Assert.Equal(
                string.Format(Strings.TheClientDoesntSupportTheGrantType, clientId, "authorization_code"),
                exception.Message);
        }

        [Fact]
        public async Task When_Passing_Valid_Request_Then_ReturnsRedirectInstruction()
        {
            const string clientId = "clientId";
            const string scope = "scope";

            var client = new Client
            {
                ResponseTypes = new[] { ResponseTypeNames.Code },
                AllowedScopes = new[] { scope },
                RedirectionUrls = new[] { new Uri(HttpsLocalhost), }
            };
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseType = ResponseTypeNames.Code,
                RedirectUrl = new Uri(HttpsLocalhost),
                ClientId = clientId,
                Scope = scope,
                Claims = null
            };

            var result = await _getAuthorizationCodeOperation
                .Execute(authorizationParameter, new ClaimsPrincipal(), client, null, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.NotNull(result.RedirectInstruction);
        }
    }
}
