namespace SimpleIdentityServer.Manager.Core.Tests.Api.Jws
{
    using Moq;
    using SimpleAuth;
    using SimpleAuth.Api.Jws;
    using SimpleAuth.Errors;
    using SimpleAuth.Exceptions;
    using SimpleAuth.Helpers;
    using SimpleAuth.Parameters;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Signature;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using Xunit;

    public class JwsActionsFixture
    {
        private JwsActions _jwsActions;
        private Mock<IJwsParser> _jwsParserStub;
        private Mock<IJsonWebKeyHelper> _jsonWebKeyHelperStub;
        private Mock<IJwsGenerator> _jwsGeneratorStub;

        [Fact]
        public async Task When_Passing_Null_Parameter_To_GetJwsInformation_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var getJwsParameter = new GetJwsParameter();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _jwsActions.GetJwsInformation(null))
                .ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _jwsActions.GetJwsInformation(getJwsParameter))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_To_CreateJws_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _jwsActions.CreateJws(null)).ConfigureAwait(false);
        }

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exceptions_Are_Thrown()
        {
            InitializeFakeObjects();
            var createJwsParameter = new CreateJwsParameter();
            var emptyCreateJwsParameter = new CreateJwsParameter
            {
                Payload = new JwsPayload()
            };

            Assert.ThrowsAsync<ArgumentNullException>(() => _jwsActions.CreateJws(null)).ConfigureAwait(false);
            Assert.ThrowsAsync<ArgumentNullException>(() => _jwsActions.CreateJws(createJwsParameter))
                .ConfigureAwait(false);
            Assert.ThrowsAsync<ArgumentNullException>(() => _jwsActions.CreateJws(emptyCreateJwsParameter))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_RS256Alg_But_No_Uri_And_Kid_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var createJwsParameter = new CreateJwsParameter
            {
                Payload = new JwsPayload(),
                Alg = JwsAlg.RS256
            };
            createJwsParameter.Payload.Add("sub", "sub");

            var exception = await Assert
                .ThrowsAsync<IdentityServerException>(async () =>
                    await _jwsActions.CreateJws(createJwsParameter).ConfigureAwait(false))
                .ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == ErrorDescriptions.TheJwsCannotBeGeneratedBecauseMissingParameters);
        }

        [Fact]
        public async Task When_Url_Is_Not_Well_Formed_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            const string url = "invalid_url";
            var createJwsParameter = new CreateJwsParameter
            {
                Payload = new JwsPayload(),
                Alg = JwsAlg.RS256,
                Kid = "kid",
                Url = url
            };
            createJwsParameter.Payload.Add("sub", "sub");

            var exception = await Assert
                .ThrowsAsync<IdentityServerException>(async () =>
                    await _jwsActions.CreateJws(createJwsParameter).ConfigureAwait(false))
                .ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == ErrorDescriptions.TheUrlIsNotWellFormed);
        }

        [Fact]
        public async Task When_There_Is_No_JsonWebKey_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            const string url = "http://google.be/";
            const string kid = "kid";
            var createJwsParameter = new CreateJwsParameter
            {
                Payload = new JwsPayload(),
                Alg = JwsAlg.RS256,
                Kid = kid,
                Url = url
            };
            createJwsParameter.Payload.Add("sub", "sub");
            _jsonWebKeyHelperStub.Setup(j => j.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(Task.FromResult<JsonWebKey>(null));

            var exception = await Assert
                .ThrowsAsync<IdentityServerException>(async () =>
                    await _jwsActions.CreateJws(createJwsParameter).ConfigureAwait(false))
                .ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, kid, url));
        }

        [Fact]
        public async Task When_Generating_Unsigned_Jws_Then_Operation_Is_Called()
        {
            InitializeFakeObjects();
            var createJwsParameter = new CreateJwsParameter
            {
                Alg = JwsAlg.none,
                Payload = new JwsPayload()
            };
            createJwsParameter.Payload.Add("sub", "sub");

            await _jwsActions.CreateJws(createJwsParameter).ConfigureAwait(false);

            _jwsGeneratorStub.Verify(j => j.Generate(createJwsParameter.Payload, JwsAlg.none, null));
        }

        [Fact]
        public async Task When_Generating_Signed_Jws_Then_Operation_Is_Called()
        {
            InitializeFakeObjects();
            const string url = "http://google.be/";
            const string kid = "kid";
            var createJwsParameter = new CreateJwsParameter
            {
                Payload = new JwsPayload(),
                Alg = JwsAlg.RS256,
                Kid = kid,
                Url = url
            };
            var jsonWebKey = new JsonWebKey();
            createJwsParameter.Payload.Add("sub", "sub");
            _jsonWebKeyHelperStub.Setup(j => j.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(Task.FromResult<JsonWebKey>(jsonWebKey));

            await _jwsActions.CreateJws(createJwsParameter).ConfigureAwait(false);

            _jwsGeneratorStub.Verify(j => j.Generate(createJwsParameter.Payload, JwsAlg.RS256, jsonWebKey));
        }

        //[Fact]
        //public async Task When_Executing_GetJwsInformation_Then_Operation_Is_Called()
        //{
        //    InitializeFakeObjects();
        //    var getJwsParameter = new GetJwsParameter
        //    {
        //        Jws = "jws"
        //    };

        //    await _jwsActions.GetJwsInformation(getJwsParameter).ConfigureAwait(false);

        //    _getJwsInformationActionStub.Verify(g => g.Execute(getJwsParameter));
        //}

        //[Fact]
        //public async Task When_Executing_CreateJws_Then_Operation_Is_Called()
        //{
        //    InitializeFakeObjects();
        //    var createJwsParameter = new CreateJwsParameter
        //    {
        //        Payload = new JwsPayload()
        //    };

        //    await _jwsActions.CreateJws(createJwsParameter).ConfigureAwait(false);

        //    _createJwsActionStub.Verify(g => g.Execute(createJwsParameter));
        //}

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var getJwsParameter = new GetJwsParameter();

            // ACTS & ASSERTS
            Assert.ThrowsAsync<AggregateException>(() => _jwsActions.GetJwsInformation(null)).ConfigureAwait(false);
            Assert.ThrowsAsync<AggregateException>(() => _jwsActions.GetJwsInformation(getJwsParameter))
                .ConfigureAwait(false);
        }

        //[Fact]
        //public async Task When_Passing_Not_Well_Formed_Url_Then_Exception_Is_Thrown()
        //{
        //    InitializeFakeObjects();
        //    const string url = "not_well_formed";
        //    var getJwsParameter = new GetJwsParameter
        //    {
        //        Url = url,
        //        Jws = "jws"
        //    };

        //    // ACTS & ASSERTS
        //    var innerException = await Assert.ThrowsAsync<IdentityServerException>(async () =>
        //            await _jwsActions.GetJwsInformation(getJwsParameter).ConfigureAwait(false))
        //        .ConfigureAwait(false);
        //    Assert.NotNull(innerException);
        //    Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
        //    Assert.True(innerException.Message == string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, url));
        //}

        [Fact]
        public async Task When_Passing_A_Not_Valid_Jws_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var getJwsParameter = new GetJwsParameter
            {
                Url = new Uri("http://google.com"),
                Jws = "jws"
            };
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(() => null);


            var innerException = await Assert.ThrowsAsync<IdentityServerException>(async () =>
                    await _jwsActions.GetJwsInformation(getJwsParameter).ConfigureAwait(false))
                .ConfigureAwait(false);
            Assert.NotNull(innerException);
            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(innerException.Message == ErrorDescriptions.TheTokenIsNotAValidJws);
        }

        [Fact]
        public async Task When_No_Uri_And_Sign_Alg_Are_Specified_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var getJwsParameter = new GetJwsParameter
            {
                Jws = "jws"
            };
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Kid = "kid",
                Alg = JwtConstants.JwsAlgNames.RS256
            };
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);


            var innerException = await Assert.ThrowsAsync<IdentityServerException>(async () =>
                    await _jwsActions.GetJwsInformation(getJwsParameter).ConfigureAwait(false))
                .ConfigureAwait(false);
            Assert.NotNull(innerException);
            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(innerException.Message == ErrorDescriptions.TheSignatureCannotBeChecked);
        }

        [Fact]
        public async Task When_JsonWebKey_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var url = new Uri("http://google.com/");
            const string kid = "kid";
            var getJwsParameter = new GetJwsParameter
            {
                Url = url,
                Jws = "jws"
            };
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Kid = kid,
                Alg = JwtConstants.JwsAlgNames.RS256
            };
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jsonWebKeyHelperStub.Setup(h => h.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(Task.FromResult<JsonWebKey>(null));


            var innerException = await Assert.ThrowsAsync<IdentityServerException>(async () =>
                    await _jwsActions.GetJwsInformation(getJwsParameter).ConfigureAwait(false))
                .ConfigureAwait(false);
            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(innerException.Message ==
                        string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, kid, url));
        }

        [Fact]
        public async Task When_The_Signature_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var url = new Uri("http://google.com/");
            const string kid = "kid";
            var getJwsParameter = new GetJwsParameter
            {
                Url = url,
                Jws = "jws"
            };
            var jsonWebKeySet = new JsonWebKeySet();
            var json = jsonWebKeySet.SerializeWithJavascript();
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Kid = kid
            };
            var jsonWebKey = new JsonWebKey
            {
                Kid = kid
            };
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jsonWebKeyHelperStub.Setup(h => h.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(Task.FromResult(jsonWebKey));
            _jwsParserStub.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(() => null);

            var innerException = await Assert.ThrowsAsync<IdentityServerException>(async () =>
                    await _jwsActions.GetJwsInformation(getJwsParameter).ConfigureAwait(false))
                .ConfigureAwait(false);
            Assert.NotNull(innerException);
            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(innerException.Message == ErrorDescriptions.TheSignatureIsNotCorrect);
        }

        [Fact]
        public async Task When_JsonWebKey_Is_Extracted_And_The_Jws_Is_Unsigned_Then_Information_Are_Returned()
        {
            var serializedRsa = string.Empty;
            using (var rsa = new RSACryptoServiceProvider())
            {
                serializedRsa = RsaExtensions.ToXmlString(rsa, true);
            };

            var url = new Uri("http://google.com/");
            const string kid = "kid";
            var getJwsParameter = new GetJwsParameter
            {
                Url = url,
                Jws = "jws"
            };
            //var jsonWebKeySet = new JsonWebKeySet();
            //var json = jsonWebKeySet.SerializeWithJavascript();
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Kid = kid
            };
            var jsonWebKey = new JsonWebKey
            {
                Kid = kid,
                Kty = KeyType.RSA,
                SerializedKey = serializedRsa
            };
            var dic = new Dictionary<string, object>
            {
                {
                    "kid", kid
                }
            };

            InitializeFakeObjects(jsonWebKey);
            var jwsPayload = new JwsPayload();
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jwsParserStub.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(jwsPayload);
            
            //_jsonWebKeyEnricherStub.Setup(j => j.GetJsonWebKeyInformation(It.IsAny<JsonWebKey>()))
            //    .Returns(dic);
            //_jsonWebKeyEnricherStub.Setup(j => j.GetPublicKeyInformation(It.IsAny<JsonWebKey>()))
            //    .Returns(() => new Dictionary<string, object>());

            var result = await _jwsActions.GetJwsInformation(getJwsParameter).ConfigureAwait(false);

            Assert.True(result.JsonWebKey.ContainsKey("kid"));
            Assert.Equal("RSA", (string)result.JsonWebKey.First().Value);
        }

        [Fact]
        public async Task When_Extracting_Information_Of_Unsigned_Jws_Then_Information_Are_Returned()
        {
            InitializeFakeObjects();
            var getJwsParameter = new GetJwsParameter
            {
                Jws = "jws"
            };
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Kid = "kid",
                Alg = JwtConstants.JwsAlgNames.NONE
            };
            var jwsPayload = new JwsPayload();
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jwsParserStub.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(jwsPayload);

            var result = await _jwsActions.GetJwsInformation(getJwsParameter).ConfigureAwait(false);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_To_GetPublicKeyInformation_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _jwsActions.GetJwsInformation(null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Unsupported_Kty_To_GetPublicKeyInformation_Then_Exception_Is_Thrown()
        {
            var jsonWebKey = new JsonWebKey
            {
                Kty = KeyType.oct
            };
            InitializeFakeObjects(jsonWebKey);

            _jwsParserStub.Setup(x => x.GetHeader(It.IsAny<string>()))
                .Returns(new JwsProtectedHeader
                {
                    Alg = JwsAlg.RS256.ToString(),
                    Kid = "kid",
                    Type = KeyType.RSA.ToString()
                });
            _jwsParserStub.Setup(x => x.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(new JwsPayload());
            var parameter = new GetJwsParameter
            {
                Url = new Uri("https://google.com"),
                Jws = "jws"
            };
            var exception = await Assert
                .ThrowsAsync<IdentityServerException>(() => _jwsActions.GetJwsInformation(parameter))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidParameterCode, exception.Code);
        }

        [Fact]
        public async Task When_Getting_Rsa_Key_Information_Then_Modulus_And_Exponent_Are_Returned()
        {
            var serializedRsa = string.Empty;
            using (var rsa = new RSACryptoServiceProvider())
            {
                serializedRsa = RsaExtensions.ToXmlString(rsa, true);
            };

            var jsonWebKey = new JsonWebKey
            {
                Kty = KeyType.RSA,
                SerializedKey = serializedRsa
            };

            InitializeFakeObjects(jsonWebKey);
            _jwsParserStub.Setup(x => x.GetHeader(It.IsAny<string>()))
                .Returns(new JwsProtectedHeader());
            _jwsParserStub.Setup(x => x.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(new JwsPayload());
            var url = new Uri("https://blah");

            var getJwsParameter = new GetJwsParameter
            {
                Jws = "jws",
                Url = url
            };
            var result = await _jwsActions.GetJwsInformation(getJwsParameter).ConfigureAwait(false);

            Assert.True(result.JsonWebKey.ContainsKey(JwtConstants.JsonWebKeyParameterNames.RsaKey.ModulusName));
            Assert.True(result.JsonWebKey.ContainsKey(JwtConstants.JsonWebKeyParameterNames.RsaKey.ExponentName));
        }

        //[Fact]
        //public void When_Passing_Null_Parameter_To_GetJsonWebKeyInformation_Then_Exception_Is_Thrown()
        //{
        //    InitializeFakeObjects();

        //    Assert.Throws<ArgumentNullException>(() => _jsonWebKeyEnricher.GetJsonWebKeyInformation(null));
        //}

        [Fact]
        public async Task When_Passing_Invalid_Kty_To_GetJsonWebKeyInformation_Then_Exception_Is_Thrown()
        {
            var jsonWebKey = new JsonWebKey
            {
                Kty = (KeyType)200
            };

            InitializeFakeObjects(jsonWebKey);
            _jwsParserStub.Setup(x => x.GetHeader(It.IsAny<string>())).Returns(new JwsProtectedHeader());
            _jwsParserStub.Setup(x => x.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(new JwsPayload());
            var parameter = new GetJwsParameter
            {
                Jws = "jws",
                Url = new Uri("https://blah")
            };
            await Assert.ThrowsAsync<ArgumentException>(() => _jwsActions.GetJwsInformation(parameter))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Invalid_Use_To_GetJsonWebKeyInformation_Then_Exception_Is_Thrown()
        {
            var jsonWebKey = new JsonWebKey
            {
                Kty = KeyType.RSA,
                Use = (Use)200
            };

            InitializeFakeObjects(jsonWebKey);
            _jwsParserStub.Setup(x => x.GetHeader(It.IsAny<string>())).Returns(new JwsProtectedHeader());
            _jwsParserStub.Setup(x => x.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(new JwsPayload());

            var parameter = new GetJwsParameter
            {
                Jws = "jws",
                Url = new Uri("https://blah")
            };
            await Assert.ThrowsAsync<ArgumentException>(() => _jwsActions.GetJwsInformation(parameter))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_JsonWebKey_To_GetJsonWebKeyInformation_Then_Information_Are_Returned()
        {
            var serializedRsa = string.Empty;
            using (var rsa = new RSACryptoServiceProvider())
            {
                serializedRsa = RsaExtensions.ToXmlString(rsa, true);
            };

            var jsonWebKey = new JsonWebKey
            {
                Kty = KeyType.RSA,
                Use = Use.Sig,
                Kid = "kid",
                SerializedKey = serializedRsa
            };

            InitializeFakeObjects(jsonWebKey);
            _jwsParserStub.Setup(x => x.GetHeader(It.IsAny<string>())).Returns(new JwsProtectedHeader());
            _jwsParserStub.Setup(x => x.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(new JwsPayload());
            
            var parameter = new GetJwsParameter
            {
                Jws = "jws",
                Url = new Uri("https://blah")
            };
            var result = await _jwsActions.GetJwsInformation(parameter).ConfigureAwait(false);

            Assert.True(result.JsonWebKey.ContainsKey(JwtConstants.JsonWebKeyParameterNames.KeyTypeName));
            Assert.True(result.JsonWebKey.ContainsKey(JwtConstants.JsonWebKeyParameterNames.UseName));
            Assert.True(result.JsonWebKey.ContainsKey(JwtConstants.JsonWebKeyParameterNames.AlgorithmName));
            Assert.True(result.JsonWebKey.ContainsKey(JwtConstants.JsonWebKeyParameterNames.KeyIdentifierName));
        }

        private void InitializeFakeObjects(JsonWebKey jwk = null)
        {
            _jwsParserStub = new Mock<IJwsParser>();

            _jsonWebKeyHelperStub = new Mock<IJsonWebKeyHelper>();
            if (jwk != null)
            {
                _jsonWebKeyHelperStub.Setup(x => x.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
                    .ReturnsAsync(jwk);
            }

            _jwsGeneratorStub = new Mock<IJwsGenerator>();
            _jwsActions = new JwsActions(
                _jwsParserStub.Object,
                _jwsGeneratorStub.Object,
                _jsonWebKeyHelperStub.Object);
        }
    }
}
