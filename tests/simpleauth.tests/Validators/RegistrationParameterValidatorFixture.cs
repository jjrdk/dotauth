namespace SimpleAuth.Tests.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Errors;
    using Exceptions;
    using Fake;
    using Repositories;
    using Shared;
    using Shared.Models;
    using Shared.Requests;
    using SimpleAuth;
    using Xunit;

    public sealed class ClientFactoryFixture
    {
        private HttpClient _httpClientFactoryFake;
        private ClientFactory _factory;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _factory.Build(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_There_Is_No_Request_Uri_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var parameter = new Client
            {
                RedirectionUrls = new[]{new Uri("https://localhost"), },
                AllowedScopes = new[] { new Scope { Name = "test" } },
                RequestUris = null
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestUriCode, ex.Code);
            Assert.Equal(string.Format(ErrorDescriptions.MissingParameter, SharedConstants.ClientNames.RequestUris), ex.Message);
        }

        //[Fact(Skip = "No longer valid test case")]
        //public async Task When_One_Request_Uri_Is_Not_Valid_Then_Exception_Is_Thrown()
        //{
        //    InitializeFakeObjects();
        //    var httpsInvalid = "https://invalid/";
        //    var parameter = new Client
        //    {
        //        RedirectionUrls = new List<Uri>
        //        {
        //            new Uri(httpsInvalid)
        //        }
        //    };

        //    var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
        //    Assert.True(ex.Code == ErrorCodes.InvalidRedirectUri);
        //    Assert.True(ex.Message == string.Format(ErrorDescriptions.TheRedirectUrlIsNotValid, httpsInvalid));
        //}

        [Fact]
        public async Task When_One_Request_Uri_Contains_A_Fragment_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var localhost = "http://localhost/#localhost";
            var parameter = new Client
            {
                RedirectionUrls = new List<Uri>
                {
                    new Uri(localhost)
                },
                AllowedScopes = new[] { new Scope { Name = "test" } },
                RequestUris = new[] { new Uri("https://localhost"), }
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRedirectUri, ex.Code);
            Assert.Equal(string.Format(ErrorDescriptions.TheRedirectUrlCannotContainsFragment, localhost), ex.Message);
        }

        [Fact]
        public async Task When_ResponseType_Is_Not_Defined_Then_Set_To_Code()
        {
            InitializeFakeObjects();
            var parameter = new Client
            {
                RedirectionUrls = new List<Uri>
                {
                    new Uri("https://google.fr")
                },
                AllowedScopes = new[] { new Scope { Name = "test" } },
                RequestUris = new[] { new Uri("https://localhost"), }
            };

            parameter = await _factory.Build(parameter).ConfigureAwait(false);

            Assert.True(parameter.ResponseTypes.Count == 1);
            Assert.Contains(ResponseTypeNames.Code, parameter.ResponseTypes);
        }

        [Fact]
        public async Task When_GrantType_Is_Not_Defined_Then_Set_To_Authorization_Code()
        {
            InitializeFakeObjects();
            var parameter = new Client
            {
                RedirectionUrls = new List<Uri>
                {
                    new Uri("https://google.fr")
                },
                AllowedScopes = new[] { new Scope { Name = "test" } },
                RequestUris = new[] { new Uri("https://localhost"), }
            };

            parameter = await _factory.Build(parameter).ConfigureAwait(false);

            Assert.NotNull(parameter);
            Assert.True(parameter.GrantTypes.Count == 1);
            Assert.Contains(GrantType.authorization_code, parameter.GrantTypes);
        }

        [Fact]
        public async Task When_Application_Type_Is_Not_Defined_Then_Set_To_Web_Application()
        {
            InitializeFakeObjects();
            var parameter = new Client
            {
                RedirectionUrls = new List<Uri>
                {
                    new Uri("https://google.fr")
                },
                AllowedScopes = new[] { new Scope { Name = "test" } },
                RequestUris = new[]{new Uri("https://localhost"), }
            };

            parameter = await _factory.Build(parameter).ConfigureAwait(false);

            Assert.NotNull(parameter);
            Assert.Equal(ApplicationTypes.web, parameter.ApplicationType);
        }

        //[Fact(Skip = "No longer valid test case")]
        //public async Task When_Logo_Uri_Is_Not_Valid_Then_Exception_Is_Thrown()
        //{
        //    InitializeFakeObjects();
        //    var parameter = new Client
        //    {
        //        RedirectionUrls = new List<Uri>
        //        {
        //            new Uri("https://google.fr")
        //        },
        //        LogoUri = new Uri("https://logo_uri")
        //    };

        //    var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
        //    Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
        //    Assert.True(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, ClientNames.LogoUri));
        //}

        //[Fact(Skip = "No longer valid test case")]
        //public async Task When_Client_Uri_Is_Not_Valid_Then_Exception_Is_Thrown()
        //{
        //    InitializeFakeObjects();
        //    var parameter = new Client
        //    {
        //        RedirectionUrls = new List<Uri>
        //        {
        //            new Uri("https://google.fr")
        //        },
        //        ClientUri = new Uri("https://client_uri")
        //    };

        //    var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
        //    Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
        //    Assert.True(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, ClientNames.ClientUri));
        //}

        //[Fact(Skip = "No longer valid test case")]
        //public async Task When_Tos_Uri_Is_Not_Valid_Then_Exception_Is_Thrown()
        //{
        //    InitializeFakeObjects();
        //    var parameter = new Client
        //    {
        //        RedirectionUrls = new List<Uri>
        //        {
        //            new Uri("https://google.fr")
        //        },
        //        TosUri = new Uri("https://tos_uri/")
        //    };

        //    var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
        //    Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
        //    Assert.True(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, ClientNames.TosUri));
        //}

        [Fact]
        public async Task When_Jwks_Uri_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var parameter = new Client
            {
                RedirectionUrls = new List<Uri>
                {
                    new Uri("https://google.fr")
                },
                JwksUri = new Uri("https://jwks_uri"),
                RequestUris = new[] { new Uri("https://localhost"), }
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Code);
            Assert.Equal(string.Format(ErrorDescriptions.TheJwksParameterCannotBeSetBecauseJwksUrlIsUsed, SharedConstants.ClientNames.JwksUri), ex.Message);
        }

        [Fact]
        public async Task When_Set_Jwks_And_Jwks_Uri_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var parameter = new Client
            {
                RedirectionUrls = new List<Uri>
                {
                    new Uri("https://google.fr")
                },
                JwksUri = new Uri("http://localhost/identity"),
                JsonWebKeys = new List<JsonWebKey>(),
                RequestUris = new[] { new Uri("https://localhost"), }
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == ErrorDescriptions.TheJwksParameterCannotBeSetBecauseJwksUrlIsUsed);
        }

        [Fact]
        public async Task When_SectorIdentifierUri_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var parameter = new Client
            {
                RedirectionUrls = new List<Uri>
                {
                    new Uri("https://google.fr")
                },
                SectorIdentifierUri = new Uri("https://sector_identifier_uri/")
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClientMetaData, ex.Code);
            Assert.Equal(ErrorDescriptions.TheSectorIdentifierUrisCannotBeRetrieved, ex.Message);
        }

        [Fact]
        public async Task When_SectorIdentifierUri_Does_Not_Have_Https_Scheme_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var parameter = new Client
            {
                RedirectionUrls = new List<Uri>
                {
                    new Uri("https://google.fr")
                },
                SectorIdentifierUri = new Uri("http://localhost/identity")
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, SharedConstants.ClientNames.SectorIdentifierUri));
        }

        [Fact]
        public async Task When_SectorIdentifierUri_Cannot_Be_Retrieved_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var parameter = new Client
            {
                RedirectionUrls = new List<Uri>
                {
                    new Uri("https://google.fr")
                },
                SectorIdentifierUri = new Uri("https://localhost/identity")
            };

            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);
            _httpClientFactoryFake = httpClientFake;

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == ErrorDescriptions.TheSectorIdentifierUrisCannotBeRetrieved);
        }

        [Fact]
        public async Task When_SectorIdentifierUri_Is_Not_A_Redirect_Uri_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var parameter = new Client
            {
                RedirectionUrls = new List<Uri>
                {
                    new Uri("https://google.fr")
                },
                SectorIdentifierUri = new Uri("https://localhost/identity")
            };

            var sectorIdentifierUris = new List<string>
            {
                "https://localhost/sector_identifier"
            };
            var json = sectorIdentifierUris.SerializeWithJavascript();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(json)
            };
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);
            _factory = new ClientFactory(httpClientFake, new DefaultScopeRepository());

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == ErrorDescriptions.OneOrMoreSectorIdentifierUriIsNotARedirectUri);
        }

        [Fact]
        public async Task When_IdTokenEncryptedResponseEnc_Is_Specified_But_Not_IdTokenEncryptedResponseAlg_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var parameter = new Client
            {
                RedirectionUrls = new List<Uri>
                {
                    new Uri("https://google.fr")
                },
                IdTokenEncryptedResponseEnc = JwtConstants.JweEncNames.A128CBC_HS256
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == ErrorDescriptions.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified);
        }

        [Fact]
        public async Task When_IdToken_Encrypted_Response_Enc_Is_Specified_And_Id_Token_Encrypted_Response_Alg_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var parameter = new Client
            {
                RedirectionUrls = new List<Uri>
                {
                    new Uri("https://google.fr")
                },
                IdTokenEncryptedResponseEnc = JwtConstants.JweEncNames.A128CBC_HS256,
                IdTokenEncryptedResponseAlg = "not_correct"
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == ErrorDescriptions.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified);
        }

        [Fact]
        public async Task When_User_Info_Encrypted_Response_Enc_Is_Specified_And_User_Info_Encrypted_Alg_Is_Not_Set_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var parameter = new Client
            {
                RedirectionUrls = new List<Uri>
                {
                    new Uri("https://google.fr")
                },
                UserInfoEncryptedResponseEnc = JwtConstants.JweEncNames.A128CBC_HS256
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == ErrorDescriptions.TheParameterUserInfoEncryptedResponseAlgMustBeSpecified);
        }

        [Fact]
        public async Task When_User_Info_Encrypted_Response_Enc_Is_Specified_And_User_Info_Encrypted_Alg_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var parameter = new Client
            {
                RedirectionUrls = new List<Uri>
                {
                    new Uri("https://google.fr")
                },
                UserInfoEncryptedResponseEnc = JwtConstants.JweEncNames.A128CBC_HS256,
                UserInfoEncryptedResponseAlg = "user_info_encrypted_response_alg_not_correct"
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == ErrorDescriptions.TheParameterUserInfoEncryptedResponseAlgMustBeSpecified);
        }

        [Fact]
        public async Task When_Request_Object_Encryption_Enc_Is_Specified_And_Request_Object_Encryption_Alg_Is_Not_Set_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var parameter = new Client
            {
                RedirectionUrls = new List<Uri>
                {
                    new Uri("https://google.fr")
                },
                RequestObjectEncryptionEnc = JwtConstants.JweEncNames.A128CBC_HS256
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == ErrorDescriptions.TheParameterRequestObjectEncryptionAlgMustBeSpecified);
        }

        [Fact]
        public async Task When_Request_Object_Encryption_Enc_Is_Specified_And_Request_Object_Encryption_Alg_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var parameter = new Client
            {
                RedirectionUrls = new List<Uri>
                {
                    new Uri("https://google.fr")
                },
                RequestObjectEncryptionEnc = JwtConstants.JweEncNames.A128CBC_HS256,
                RequestObjectEncryptionAlg = "request_object_encryption_alg_not_valid"
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == ErrorDescriptions.TheParameterRequestObjectEncryptionAlgMustBeSpecified);
        }

        //[Fact(Skip = "No longer valid test case")]
        //public async Task When_InitiateLoginUri_Is_Not_Valid_Then_Exception_Is_Thrown()
        //{
        //    InitializeFakeObjects();
        //    var parameter = new Client
        //    {
        //        RedirectionUrls = new List<Uri>
        //        {
        //            new Uri("https://google.fr")
        //        },
        //        InitiateLoginUri = new Uri("https://sector_identifier_uri")
        //    };

        //    var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
        //    Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
        //    Assert.True(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, ClientNames.InitiateLoginUri));
        //}

        [Fact]
        public async Task When_InitiateLoginUri_Does_Not_Have_Https_Scheme_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var parameter = new Client
            {
                RedirectionUrls = new List<Uri>
                {
                    new Uri("https://google.fr")
                },
                InitiateLoginUri = new Uri("http://localhost/identity")
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, SharedConstants.ClientNames.InitiateLoginUri));
        }

        //[Fact(Skip = "No longer valid test case")]
        //public async Task When_Passing_One_Not_Valid_Request_Uri_Then_Exception_Is_Thrown()
        //{
        //    InitializeFakeObjects();
        //    var parameter = new Client
        //    {
        //        RedirectionUrls = new List<Uri>
        //        {
        //            new Uri("https://google.fr")
        //        },
        //        RequestUris = new List<Uri>
        //        {
        //            new Uri("https://not_valid_uri")
        //        }
        //    };

        //    var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _factory.Build(parameter)).ConfigureAwait(false);
        //    Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
        //    Assert.True(ex.Message == ErrorDescriptions.OneOfTheRequestUriIsNotValid);
        //}

        [Fact]
        public async Task When_Passing_Valid_Request_Then_No_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var parameter = new Client
            {
                RedirectionUrls = new List<Uri>
                {
                    new Uri("http://localhost")
                },
                AllowedScopes = new[] { new Scope { Name = "openid" } },
                ApplicationType = ApplicationTypes.native,
                JsonWebKeys = new List<JsonWebKey>(), //new JsonWebKeySet(),
                IdTokenEncryptedResponseAlg = JwtConstants.JweAlgNames.A128KW,
                IdTokenEncryptedResponseEnc = JwtConstants.JweEncNames.A128CBC_HS256,
                UserInfoEncryptedResponseAlg = JwtConstants.JweAlgNames.A128KW,
                UserInfoEncryptedResponseEnc = JwtConstants.JweEncNames.A128CBC_HS256,
                RequestObjectEncryptionAlg = JwtConstants.JweAlgNames.A128KW,
                RequestObjectEncryptionEnc = JwtConstants.JweEncNames.A128CBC_HS256,
                RequestUris = new List<Uri>
                {
                    new Uri("http://localhost")
                },
                SectorIdentifierUri = new Uri("https://localhost")
            };

            var sectorIdentifierUris = new List<string>
            {
                "http://localhost"
            };
            var json = sectorIdentifierUris.SerializeWithJavascript();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(json)
            };
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);

            _factory = new ClientFactory(httpClientFake, new DefaultScopeRepository());

            var ex = await Record.ExceptionAsync(() => _factory.Build(parameter)).ConfigureAwait(false);
            Assert.Null(ex);
        }

        private void InitializeFakeObjects()
        {
            _httpClientFactoryFake = new HttpClient();
            _factory = new ClientFactory(
                _httpClientFactoryFake,
                new DefaultScopeRepository(new[] { new Scope { Name = "test" } }));
        }
    }
}
