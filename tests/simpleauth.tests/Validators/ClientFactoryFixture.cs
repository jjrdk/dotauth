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
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Divergic.Logging.Xunit;
    using SimpleAuth.Extensions;
    using SimpleAuth.Properties;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class ClientFactoryFixture
    {
        private readonly ITestOutputHelper _outputHelper;
        private HttpClient _httpClientFake;
        private ClientFactory _factory;

        public ClientFactoryFixture(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _httpClientFake = new HttpClient();
            _factory = new ClientFactory(
                new TestHttpClientFactory(_httpClientFake),
                new InMemoryScopeRepository(new[] { new Scope { Name = "test" } }),
                s => s.DeserializeWithJavascript<Uri[]>(),
                new TestOutputLogger("test", outputHelper));
        }
        
        [Fact]
        public async Task When_One_Request_Uri_Contains_A_Fragment_Then_Exception_Is_Thrown()
        {
            var localhost = "http://localhost/#localhost";
            var parameter = new Client
            {
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                RedirectionUrls = new[] { new Uri(localhost) },
                AllowedScopes = new[] { "test" },
                RequestUris = new[] { new Uri("https://localhost"), }
            };

            var ex = await _factory.Build(parameter).ConfigureAwait(false) as Option<Client>.Error;
            Assert.Equal(ErrorCodes.InvalidRedirectUri, ex.Details.Title);
            Assert.Equal(string.Format(Strings.TheRedirectUrlCannotContainsFragment, localhost), ex.Details.Detail);
        }

        [Fact]
        public async Task When_ResponseType_Is_Not_Defined_Then_Set_To_Code()
        {
            var parameter = new Client
            {
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                RedirectionUrls = new[] { new Uri("https://google.com") },
                ResponseTypes = Array.Empty<string>(),
                AllowedScopes = new[] { "test" },
                RequestUris = new[] { new Uri("https://localhost"), }
            };

            parameter = (await _factory.Build(parameter).ConfigureAwait(false) as Option<Client>.Result)!.Item;

            Assert.Single(parameter.ResponseTypes);
            Assert.Contains(ResponseTypeNames.Code, parameter.ResponseTypes);
        }

        [Fact]
        public async Task When_GrantType_Is_Not_Defined_Then_Set_To_Authorization_Code()
        {
            var parameter = new Client
            {
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                RedirectionUrls = new[] { new Uri("https://google.com") },
                AllowedScopes = new[] { "test" },
                RequestUris = new[] { new Uri("https://localhost"), }
            };

            parameter = (await _factory.Build(parameter).ConfigureAwait(false) as Option<Client>.Result)!.Item;

            Assert.Single(parameter.GrantTypes);
            Assert.Contains(GrantTypes.AuthorizationCode, parameter.GrantTypes);
        }

        [Fact]
        public async Task When_Application_Type_Is_Not_Defined_Then_Set_To_Web_Application()
        {
            var parameter = new Client
            {
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                RedirectionUrls = new[] { new Uri("https://google.com") },
                AllowedScopes = new[] { "test" },
                RequestUris = new[] { new Uri("https://localhost"), }
            };

            parameter = (await _factory.Build(parameter).ConfigureAwait(false) as Option<Client>.Result)!.Item;

            Assert.Equal(ApplicationTypes.Web, parameter.ApplicationType);
        }

        [Fact]
        public async Task When_SectorIdentifierUri_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new[] { new Uri("https://google.com") },
                SectorIdentifierUri = new Uri("https://sector_identifier_uri/")
            };

            var ex = await _factory.Build(parameter).ConfigureAwait(false) as Option<Client>.Error;
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Details.Title);
            Assert.Equal(Strings.TheSectorIdentifierUrisCannotBeRetrieved, ex.Details.Detail);
        }

        [Fact]
        public async Task When_SectorIdentifierUri_Does_Not_Have_Https_Scheme_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new[] { new Uri("https://google.com") },
                SectorIdentifierUri = new Uri("http://localhost/identity")
            };

            var ex = await _factory.Build(parameter).ConfigureAwait(false) as Option<Client>.Error;
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Details.Title);
            Assert.Equal(string.Format(Strings.ParameterIsNotCorrect, "sector_identifier_uri"), ex.Details.Detail);
        }

        [Fact]
        public async Task When_SectorIdentifierUri_Cannot_Be_Retrieved_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new[] { new Uri("https://google.com") },
                SectorIdentifierUri = new Uri("https://localhost/identity")
            };

            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);
            _httpClientFake = httpClientFake;

            var ex = await _factory.Build(parameter).ConfigureAwait(false) as Option<Client>.Error;

            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Details.Title);
            Assert.Equal(Strings.TheSectorIdentifierUrisCannotBeRetrieved, ex.Details.Detail);
        }

        [Fact]
        public async Task When_SectorIdentifierUri_Is_Not_A_Redirect_Uri_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new[] { new Uri("https://google.com") },
                SectorIdentifierUri = new Uri("https://localhost/identity")
            };

            var sectorIdentifierUris = new List<string> { "https://localhost/sector_identifier" };
            var json = sectorIdentifierUris.SerializeWithJavascript();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(json)
            };
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);
            _factory = new ClientFactory(
                new TestHttpClientFactory(httpClientFake),
                new InMemoryScopeRepository(),
                s => s.DeserializeWithJavascript<Uri[]>(),
                new TestOutputLogger("test", _outputHelper));

            var ex = await _factory.Build(parameter).ConfigureAwait(false) as Option<Client>.Error;
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Details.Title);
            Assert.Equal(Strings.OneOrMoreSectorIdentifierUriIsNotARedirectUri, ex.Details.Detail);
        }

        [Fact]
        public async Task
            When_IdTokenEncryptedResponseEnc_Is_Specified_But_Not_IdTokenEncryptedResponseAlg_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new[] { new Uri("https://google.com") },
                IdTokenEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256
            };

            var ex = await _factory.Build(parameter).ConfigureAwait(false) as Option<Client>.Error;

            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Details.Title);
            Assert.Equal(Strings.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified, ex.Details.Detail);
        }

        [Fact]
        public async Task
            When_IdToken_Encrypted_Response_Enc_Is_Specified_And_Id_Token_Encrypted_Response_Alg_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                AllowedScopes = new[] { "test" },
                RequestUris = new[] { new Uri("https://localhost"), },
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                RedirectionUrls = new[] { new Uri("https://google.com") },
                IdTokenEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256
            };

            var ex = await _factory.Build(parameter).ConfigureAwait(false) as Option<Client>.Error;
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Details.Title);
            Assert.Equal(Strings.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified, ex.Details.Detail);
        }

        [Fact]
        public async Task
            When_User_Info_Encrypted_Response_Enc_Is_Specified_And_User_Info_Encrypted_Alg_Is_Not_Set_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new[] { new Uri("https://google.com") },
                UserInfoEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256
            };

            var ex = await _factory.Build(parameter).ConfigureAwait(false) as Option<Client>.Error;
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Details.Title);
            Assert.Equal(Strings.TheParameterUserInfoEncryptedResponseAlgMustBeSpecified, ex.Details.Detail);
        }

        [Fact]
        public async Task
            When_User_Info_Encrypted_Response_Enc_Is_Specified_And_User_Info_Encrypted_Alg_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new[] { new Uri("https://google.com") },
                UserInfoEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256,
                //UserInfoEncryptedResponseAlg = "user_info_encrypted_response_alg_not_correct"
            };

            var ex = await _factory.Build(parameter).ConfigureAwait(false) as Option<Client>.Error;
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Details.Title);
            Assert.Equal(Strings.TheParameterUserInfoEncryptedResponseAlgMustBeSpecified, ex.Details.Detail);
        }

        [Fact]
        public async Task
            When_Request_Object_Encryption_Enc_Is_Specified_And_Request_Object_Encryption_Alg_Is_Not_Set_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new[] { new Uri("https://google.com") },
                RequestObjectEncryptionEnc = SecurityAlgorithms.Aes128CbcHmacSha256
            };

            var ex = await _factory.Build(parameter).ConfigureAwait(false) as Option<Client>.Error;
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Details.Title);
            Assert.Equal(Strings.TheParameterRequestObjectEncryptionAlgMustBeSpecified, ex.Details.Detail);
        }

        [Fact]
        public async Task
            When_Request_Object_Encryption_Enc_Is_Specified_And_Request_Object_Encryption_Alg_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                AllowedScopes = new[] { "test" },
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                RequestUris = new[] { new Uri("https://localhost") },
                RedirectionUrls = new[] { new Uri("https://google.com") },
                RequestObjectEncryptionEnc = SecurityAlgorithms.Aes128CbcHmacSha256,
            };

            var ex = await _factory.Build(parameter).ConfigureAwait(false) as Option<Client>.Error;
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Details.Title);
            Assert.Equal(Strings.TheParameterRequestObjectEncryptionAlgMustBeSpecified, ex.Details.Detail);
        }

        [Fact]
        public async Task When_InitiateLoginUri_Does_Not_Have_Https_Scheme_Then_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new[] { new Uri("https://google.com") },
                InitiateLoginUri = new Uri("http://localhost/identity")
            };

            var ex = await _factory.Build(parameter).ConfigureAwait(false) as Option<Client>.Error;
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Details.Title);
            Assert.Equal(string.Format(Strings.ParameterIsNotCorrect, "initiate_login_uri"), ex.Details.Detail);
        }

        [Fact]
        public async Task When_Passing_Valid_Request_Then_No_Exception_Is_Thrown()
        {
            var parameter = new Client
            {
                RedirectionUrls = new[] { new Uri("http://localhost") },
                AllowedScopes = new[] { "openid" },
                ApplicationType = ApplicationTypes.Native,
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(), //new JsonWebKeySet(),
                IdTokenEncryptedResponseAlg = SecurityAlgorithms.Aes128KW,
                IdTokenEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256,
                UserInfoEncryptedResponseAlg = SecurityAlgorithms.Aes128KW,
                UserInfoEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256,
                RequestObjectEncryptionAlg = SecurityAlgorithms.Aes128KW,
                RequestObjectEncryptionEnc = SecurityAlgorithms.Aes128CbcHmacSha256,
                RequestUris = new[] { new Uri("http://localhost") },
                SectorIdentifierUri = new Uri("https://localhost")
            };

            var sectorIdentifierUris = new List<string> { "http://localhost" };
            var json = sectorIdentifierUris.SerializeWithJavascript();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(json)
            };
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);

            _factory = new ClientFactory(
                new TestHttpClientFactory(httpClientFake),
                new InMemoryScopeRepository(),
                s => s.DeserializeWithJavascript<Uri[]>(),
                new TestOutputLogger("test", _outputHelper));

            var ex = await Record.ExceptionAsync(() => _factory.Build(parameter)).ConfigureAwait(false);
            Assert.Null(ex);
        }
    }
}
