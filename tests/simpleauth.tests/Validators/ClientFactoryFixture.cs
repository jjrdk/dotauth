namespace SimpleAuth.Tests.Validators
{
    using Fake;
    using Helpers;
    using Microsoft.IdentityModel.Tokens;
    using Repositories;
    using Shared;
    using Shared.Models;
    using SimpleAuth;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class ClientFactoryFixture
    {
        private HttpClient _httpClientFake;
        private ClientFactory _factory;

        public ClientFactoryFixture()
        {
            _httpClientFake = new HttpClient();
            _factory = new ClientFactory(
                _httpClientFake,
                new InMemoryScopeRepository(new[] {new Scope {Name = "test"}}),
                s => s.DeserializeWithJavascript<Uri[]>());
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _factory.Build(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_There_Is_No_Request_Uri_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new[] {new Uri("https://localhost"),},
                AllowedScopes = new[] {"test"},
                RequestUris = null
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestUriCode, ex.Code);
            Assert.Equal(string.Format(ErrorDescriptions.MissingParameter, "request_uris"), ex.Message);
        }

        [Fact]
        public async Task When_One_Request_Uri_Contains_A_Fragment_Then_Exception_Is_Thrown()
        {
            var localhost = "http://localhost/#localhost";
            var parameter = new Client
            {
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                RedirectionUrls = new [] {new Uri(localhost)},
                AllowedScopes = new[] {"test"},
                RequestUris = new[] {new Uri("https://localhost"),}
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRedirectUri, ex.Code);
            Assert.Equal(string.Format(ErrorDescriptions.TheRedirectUrlCannotContainsFragment, localhost), ex.Message);
        }

        [Fact]
        public async Task When_ResponseType_Is_Not_Defined_Then_Set_To_Code()
        {
            var parameter = new Client
            {
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                RedirectionUrls = new [] {new Uri("https://google.com")},
                ResponseTypes = Array.Empty<string>(),
                AllowedScopes = new[] {"test"},
                RequestUris = new[] {new Uri("https://localhost"),}
            };

            parameter = await _factory.Build(parameter).ConfigureAwait(false);

            Assert.Single(parameter.ResponseTypes);
            Assert.Contains(ResponseTypeNames.Code, parameter.ResponseTypes);
        }

        [Fact]
        public async Task When_GrantType_Is_Not_Defined_Then_Set_To_Authorization_Code()
        {
            var parameter = new Client
            {
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                RedirectionUrls = new [] {new Uri("https://google.com")},
                AllowedScopes = new[] {"test"},
                RequestUris = new[] {new Uri("https://localhost"),}
            };

            parameter = await _factory.Build(parameter).ConfigureAwait(false);

            Assert.NotNull(parameter);
            Assert.Single(parameter.GrantTypes);
            Assert.Contains(GrantTypes.AuthorizationCode, parameter.GrantTypes);
        }

        [Fact]
        public async Task When_Application_Type_Is_Not_Defined_Then_Set_To_Web_Application()
        {
            var parameter = new Client
            {
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                RedirectionUrls = new [] {new Uri("https://google.com")},
                AllowedScopes = new[] {"test"},
                RequestUris = new[] {new Uri("https://localhost"),}
            };

            parameter = await _factory.Build(parameter).ConfigureAwait(false);

            Assert.NotNull(parameter);
            Assert.Equal(ApplicationTypes.Web, parameter.ApplicationType);
        }

        [Fact]
        public async Task When_SectorIdentifierUri_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new [] {new Uri("https://google.com")},
                SectorIdentifierUri = new Uri("https://sector_identifier_uri/")
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Code);
            Assert.Equal(ErrorDescriptions.TheSectorIdentifierUrisCannotBeRetrieved, ex.Message);
        }

        [Fact]
        public async Task When_SectorIdentifierUri_Does_Not_Have_Https_Scheme_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new [] {new Uri("https://google.com")},
                SectorIdentifierUri = new Uri("http://localhost/identity")
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Code);
            Assert.Equal(string.Format(ErrorDescriptions.ParameterIsNotCorrect, "sector_identifier_uri"), ex.Message);
        }

        [Fact]
        public async Task When_SectorIdentifierUri_Cannot_Be_Retrieved_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new [] {new Uri("https://google.com")},
                SectorIdentifierUri = new Uri("https://localhost/identity")
            };

            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);
            _httpClientFake = httpClientFake;

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Code);
            Assert.Equal(ErrorDescriptions.TheSectorIdentifierUrisCannotBeRetrieved, ex.Message);
        }

        [Fact]
        public async Task When_SectorIdentifierUri_Is_Not_A_Redirect_Uri_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new [] {new Uri("https://google.com")},
                SectorIdentifierUri = new Uri("https://localhost/identity")
            };

            var sectorIdentifierUris = new List<string> {"https://localhost/sector_identifier"};
            var json = sectorIdentifierUris.SerializeWithJavascript();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(json)
            };
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);
            _factory = new ClientFactory(
                httpClientFake,
                new InMemoryScopeRepository(),
                s => s.DeserializeWithJavascript<Uri[]>());

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Code);
            Assert.Equal(ErrorDescriptions.OneOrMoreSectorIdentifierUriIsNotARedirectUri, ex.Message);
        }

        [Fact]
        public async Task
            When_IdTokenEncryptedResponseEnc_Is_Specified_But_Not_IdTokenEncryptedResponseAlg_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new [] {new Uri("https://google.com")},
                IdTokenEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Code);
            Assert.Equal(ErrorDescriptions.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified, ex.Message);
        }

        [Fact]
        public async Task
            When_IdToken_Encrypted_Response_Enc_Is_Specified_And_Id_Token_Encrypted_Response_Alg_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                AllowedScopes = new[] {"test"},
                RequestUris = new[] {new Uri("https://localhost"),},
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                RedirectionUrls = new [] {new Uri("https://google.com")},
                IdTokenEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Code);
            Assert.Equal(ErrorDescriptions.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified, ex.Message);
        }

        [Fact]
        public async Task
            When_User_Info_Encrypted_Response_Enc_Is_Specified_And_User_Info_Encrypted_Alg_Is_Not_Set_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new [] {new Uri("https://google.com")},
                UserInfoEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Code);
            Assert.Equal(ErrorDescriptions.TheParameterUserInfoEncryptedResponseAlgMustBeSpecified, ex.Message);
        }

        [Fact]
        public async Task
            When_User_Info_Encrypted_Response_Enc_Is_Specified_And_User_Info_Encrypted_Alg_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new [] {new Uri("https://google.com")},
                UserInfoEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256,
                //UserInfoEncryptedResponseAlg = "user_info_encrypted_response_alg_not_correct"
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Code);
            Assert.Equal(ErrorDescriptions.TheParameterUserInfoEncryptedResponseAlgMustBeSpecified, ex.Message);
        }

        [Fact]
        public async Task
            When_Request_Object_Encryption_Enc_Is_Specified_And_Request_Object_Encryption_Alg_Is_Not_Set_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new [] {new Uri("https://google.com")},
                RequestObjectEncryptionEnc = SecurityAlgorithms.Aes128CbcHmacSha256
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Code);
            Assert.Equal(ErrorDescriptions.TheParameterRequestObjectEncryptionAlgMustBeSpecified, ex.Message);
        }

        [Fact]
        public async Task
            When_Request_Object_Encryption_Enc_Is_Specified_And_Request_Object_Encryption_Alg_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                AllowedScopes = new[] {"test"},
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                RequestUris = new[] {new Uri("https://localhost")},
                RedirectionUrls = new [] {new Uri("https://google.com")},
                RequestObjectEncryptionEnc = SecurityAlgorithms.Aes128CbcHmacSha256,
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Code);
            Assert.Equal(ErrorDescriptions.TheParameterRequestObjectEncryptionAlgMustBeSpecified, ex.Message);
        }

        [Fact]
        public async Task When_InitiateLoginUri_Does_Not_Have_Https_Scheme_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new [] {new Uri("https://google.com")},
                InitiateLoginUri = new Uri("http://localhost/identity")
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Code);
            Assert.Equal(string.Format(ErrorDescriptions.ParameterIsNotCorrect, "initiate_login_uri"), ex.Message);
        }

        [Fact]
        public async Task When_Passing_Valid_Request_Then_No_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new [] {new Uri("http://localhost")},
                AllowedScopes = new[] {"openid"},
                ApplicationType = ApplicationTypes.Native,
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(), //new JsonWebKeySet(),
                IdTokenEncryptedResponseAlg = SecurityAlgorithms.Aes128KW,
                IdTokenEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256,
                UserInfoEncryptedResponseAlg = SecurityAlgorithms.Aes128KW,
                UserInfoEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256,
                RequestObjectEncryptionAlg = SecurityAlgorithms.Aes128KW,
                RequestObjectEncryptionEnc = SecurityAlgorithms.Aes128CbcHmacSha256,
                RequestUris = new [] {new Uri("http://localhost")},
                SectorIdentifierUri = new Uri("https://localhost")
            };

            var sectorIdentifierUris = new List<string> {"http://localhost"};
            var json = sectorIdentifierUris.SerializeWithJavascript();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(json)
            };
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);

            _factory = new ClientFactory(
                httpClientFake,
                new InMemoryScopeRepository(),
                s => s.DeserializeWithJavascript<Uri[]>());

            var ex = await Record.ExceptionAsync(() => _factory.Build(parameter)).ConfigureAwait(false);
            Assert.Null(ex);
        }
    }
}
