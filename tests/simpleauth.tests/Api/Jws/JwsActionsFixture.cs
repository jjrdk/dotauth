//namespace SimpleAuth.Tests.Api.Jws
//{
//    using Errors;
//    using Exceptions;
//    using Microsoft.IdentityModel.Tokens;
//    using Moq;
//    using Parameters;
//    using Shared;
//    using SimpleAuth;
//    using SimpleAuth.Helpers;
//    using System;
//    using System.Linq;
//    using System.Security.Cryptography;
//    using System.Threading.Tasks;
//    using Xunit;

//    public class JwsActionsFixture
//    {
//        private JwsActions _jwsActions;
//        private Mock<IJsonWebKeyHelper> _jsonWebKeyHelperStub;

//        [Fact]
//        public async Task When_Passing_Null_Parameter_To_GetJwsInformation_Then_Exception_Is_Thrown()
//        {
//            InitializeFakeObjects();
//            var getJwsParameter = new GetJwsParameter();


//            await Assert.ThrowsAsync<ArgumentNullException>(() => _jwsActions.GetJwsInformation(null))
//                .ConfigureAwait(false);
//            await Assert.ThrowsAsync<ArgumentNullException>(() => _jwsActions.GetJwsInformation(getJwsParameter))
//                .ConfigureAwait(false);
//        }

//        [Fact]
//        public async Task When_Passing_Null_Parameter_To_CreateJws_Then_Exception_Is_Thrown()
//        {
//            InitializeFakeObjects();


//            await Assert.ThrowsAsync<ArgumentNullException>(() => _jwsActions.CreateJws(null)).ConfigureAwait(false);
//        }

//        [Fact]
//        public void When_Passing_Null_Parameter_Then_Exceptions_Are_Thrown()
//        {
//            InitializeFakeObjects();
//            var createJwsParameter = new CreateJwsParameter();
//            var emptyCreateJwsParameter = new CreateJwsParameter
//            {
//                Payload = new JwtSecurityToken()
//            };

//            Assert.ThrowsAsync<ArgumentNullException>(() => _jwsActions.CreateJws(null)).ConfigureAwait(false);
//            Assert.ThrowsAsync<ArgumentNullException>(() => _jwsActions.CreateJws(createJwsParameter))
//                .ConfigureAwait(false);
//            Assert.ThrowsAsync<ArgumentNullException>(() => _jwsActions.CreateJws(emptyCreateJwsParameter))
//                .ConfigureAwait(false);
//        }

//        [Fact]
//        public async Task When_Passing_RsaSha256Alg_But_No_Uri_And_Kid_Then_Exception_Is_Thrown()
//        {
//            InitializeFakeObjects();
//            var createJwsParameter = new CreateJwsParameter
//            {
//                Payload = new JwtSecurityToken(),
//                Alg = SecurityAlgorithms.RsaSha256
//            };
//            createJwsParameter.Payload.Add("sub", "sub");

//            var exception = await Assert
//                .ThrowsAsync<SimpleAuthException>(async () =>
//                    await _jwsActions.CreateJws(createJwsParameter).ConfigureAwait(false))
//                .ConfigureAwait(false);
//            Assert.NotNull(exception);
//            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
//            Assert.True(exception.Message == ErrorDescriptions.TheJwsCannotBeGeneratedBecauseMissingParameters);
//        }

//        [Fact]
//        public async Task When_Url_Is_Not_Well_Formed_Then_Exception_Is_Thrown()
//        {
//            InitializeFakeObjects();
//            const string url = "invalid_url";
//            var createJwsParameter = new CreateJwsParameter
//            {
//                Payload = new JwtSecurityToken(),
//                Alg = SecurityAlgorithms.RsaSha256,
//                Kid = "kid",
//                Url = url
//            };
//            createJwsParameter.Payload.Add("sub", "sub");

//            var exception = await Assert
//                .ThrowsAsync<SimpleAuthException>(async () =>
//                    await _jwsActions.CreateJws(createJwsParameter).ConfigureAwait(false))
//                .ConfigureAwait(false);
//            Assert.NotNull(exception);
//            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
//            Assert.True(exception.Message == ErrorDescriptions.TheUrlIsNotWellFormed);
//        }

//        [Fact]
//        public async Task When_There_Is_No_JsonWebKey_Then_Exception_Is_Thrown()
//        {
//            InitializeFakeObjects();
//            const string url = "http://google.be/";
//            const string kid = "kid";
//            var createJwsParameter = new CreateJwsParameter
//            {
//                Payload = new JwtSecurityToken(),
//                Alg = SecurityAlgorithms.RsaSha256,
//                Kid = kid,
//                Url = url
//            };
//            createJwsParameter.Payload.Add("sub", "sub");
//            _jsonWebKeyHelperStub.Setup(j => j.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
//                .Returns(Task.FromResult<JsonWebKey>(null));

//            var exception = await Assert
//                .ThrowsAsync<SimpleAuthException>(async () =>
//                    await _jwsActions.CreateJws(createJwsParameter).ConfigureAwait(false))
//                .ConfigureAwait(false);
//            Assert.NotNull(exception);
//            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
//            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, kid, url));
//        }

//        //[Fact]
//        //public async Task When_Generating_Unsigned_Jws_Then_Operation_Is_Called()
//        //{
//        //    InitializeFakeObjects();
//        //    var createJwsParameter = new CreateJwsParameter
//        //    {
//        //        Alg = SecurityAlgorithms.None,
//        //        Payload = new JwtSecurityToken()
//        //    };
//        //    createJwsParameter.Payload.Add("sub", "sub");

//        //    await _jwsActions.CreateJws(createJwsParameter).ConfigureAwait(false);

//        //    _jwsGeneratorStub.Verify(j => j.Generate(createJwsParameter.Payload, SecurityAlgorithms.None, null));
//        //}

//        //[Fact]
//        //public async Task When_Generating_Signed_Jws_Then_Operation_Is_Called()
//        //{
//        //    InitializeFakeObjects();
//        //    const string url = "http://google.be/";
//        //    const string kid = "kid";
//        //    var createJwsParameter = new CreateJwsParameter
//        //    {
//        //        Payload = new JwtSecurityToken(),
//        //        Alg = SecurityAlgorithms.RsaSha256,
//        //        Kid = kid,
//        //        Url = url
//        //    };
//        //    var jsonWebKey = new JsonWebKey();
//        //    createJwsParameter.Payload.Add("sub", "sub");
//        //    _jsonWebKeyHelperStub.Setup(j => j.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
//        //        .Returns(Task.FromResult<JsonWebKey>(jsonWebKey));

//        //    await _jwsActions.CreateJws(createJwsParameter).ConfigureAwait(false);

//        //    _jwsGeneratorStub.Verify(j => j.Generate(createJwsParameter.Payload, SecurityAlgorithms.RsaSha256, jsonWebKey));
//        //}

//        //[Fact]
//        //public async Task When_Executing_GetJwsInformation_Then_Operation_Is_Called()
//        //{
//        //    InitializeFakeObjects();
//        //    var getJwsParameter = new GetJwsParameter
//        //    {
//        //        Jws = "jws"
//        //    };

//        //    await _jwsActions.GetJwsInformation(getJwsParameter).ConfigureAwait(false);

//        //    _getJwsInformationActionStub.Verify(g => g.Execute(getJwsParameter));
//        //}

//        //[Fact]
//        //public async Task When_Executing_CreateJws_Then_Operation_Is_Called()
//        //{
//        //    InitializeFakeObjects();
//        //    var createJwsParameter = new CreateJwsParameter
//        //    {
//        //        Payload = new JwtSecurityToken()
//        //    };

//        //    await _jwsActions.CreateJws(createJwsParameter).ConfigureAwait(false);

//        //    _createJwsActionStub.Verify(g => g.Execute(createJwsParameter));
//        //}

//        [Fact]
//        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
//        {
//            InitializeFakeObjects();
//            var getJwsParameter = new GetJwsParameter();


//            Assert.ThrowsAsync<AggregateException>(() => _jwsActions.GetJwsInformation(null)).ConfigureAwait(false);
//            Assert.ThrowsAsync<AggregateException>(() => _jwsActions.GetJwsInformation(getJwsParameter))
//                .ConfigureAwait(false);
//        }

//        //[Fact]
//        //public async Task When_Passing_Not_Well_Formed_Url_Then_Exception_Is_Thrown()
//        //{
//        //    InitializeFakeObjects();
//        //    const string url = "not_well_formed";
//        //    var getJwsParameter = new GetJwsParameter
//        //    {
//        //        Url = url,
//        //        Jws = "jws"
//        //    };

//        //    
//        //    var innerException = await Assert.ThrowsAsync<SimpleAuthException>(async () =>
//        //            await _jwsActions.GetJwsInformation(getJwsParameter).ConfigureAwait(false))
//        //        .ConfigureAwait(false);
//        //    Assert.NotNull(innerException);
//        //    Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
//        //    Assert.True(innerException.Message == string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, url));
//        //}

//        [Fact]
//        public async Task When_Passing_A_Not_Valid_Jws_Then_Exception_Is_Thrown()
//        {
//            InitializeFakeObjects();
//            var getJwsParameter = new GetJwsParameter
//            {
//                Url = new Uri("http://google.com"),
//                Jws = "jws"
//            };
//            var innerException = await Assert.ThrowsAsync<SimpleAuthException>(async () =>
//                    await _jwsActions.GetJwsInformation(getJwsParameter).ConfigureAwait(false))
//                .ConfigureAwait(false);
//            Assert.NotNull(innerException);
//            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
//            Assert.True(innerException.Message == ErrorDescriptions.TheTokenIsNotAValidJws);
//        }

//        [Fact]
//        public async Task When_No_Uri_And_Sign_Alg_Are_Specified_Then_Exception_Is_Thrown()
//        {
//            InitializeFakeObjects();
//            var getJwsParameter = new GetJwsParameter
//            {
//                Jws = "jws"
//            };

//            var innerException = await Assert.ThrowsAsync<SimpleAuthException>(async () =>
//                    await _jwsActions.GetJwsInformation(getJwsParameter).ConfigureAwait(false))
//                .ConfigureAwait(false);
//            Assert.NotNull(innerException);
//            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
//            Assert.True(innerException.Message == ErrorDescriptions.TheSignatureCannotBeChecked);
//        }

//        [Fact]
//        public async Task When_JsonWebKey_Does_Not_Exist_Then_Exception_Is_Thrown()
//        {
//            InitializeFakeObjects();
//            var url = new Uri("http://google.com/");
//            const string kid = "kid";
//            var getJwsParameter = new GetJwsParameter
//            {
//                Url = url,
//                Jws = "jws"
//            };

//            _jsonWebKeyHelperStub.Setup(h => h.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
//                .Returns(Task.FromResult<JsonWebKey>(null));


//            var innerException = await Assert.ThrowsAsync<SimpleAuthException>(async () =>
//                    await _jwsActions.GetJwsInformation(getJwsParameter).ConfigureAwait(false))
//                .ConfigureAwait(false);
//            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
//            Assert.True(innerException.Message ==
//                        string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, kid, url));
//        }

//        [Fact]
//        public async Task When_The_Signature_Is_Not_Valid_Then_Exception_Is_Thrown()
//        {
//            InitializeFakeObjects();
//            var url = new Uri("http://google.com/");
//            const string kid = "kid";
//            var getJwsParameter = new GetJwsParameter
//            {
//                Url = url,
//                Jws = "jws"
//            };

//            var jsonWebKey = new JsonWebKey
//            {
//                Kid = kid
//            };

//            _jsonWebKeyHelperStub.Setup(h => h.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
//                .Returns(Task.FromResult(jsonWebKey));

//            var innerException = await Assert.ThrowsAsync<SimpleAuthException>(async () =>
//                    await _jwsActions.GetJwsInformation(getJwsParameter).ConfigureAwait(false))
//                .ConfigureAwait(false);
//            Assert.NotNull(innerException);
//            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
//            Assert.True(innerException.Message == ErrorDescriptions.TheSignatureIsNotCorrect);
//        }

//        [Fact]
//        public async Task When_JsonWebKey_Is_Extracted_And_The_Jws_Is_Unsigned_Then_Information_Are_Returned()
//        {
//            using (var rsa = new RSACryptoServiceProvider())
//            {

//                var url = new Uri("http://google.com/");
//                var getJwsParameter = new GetJwsParameter
//                {
//                    Url = url,
//                    Jws = "jws"
//                };

//                var jsonWebKey = JsonWebKeyConverter.ConvertFromRSASecurityKey(new RsaSecurityKey(rsa));
//                InitializeFakeObjects(jsonWebKey);

//                var result = await _jwsActions.GetJwsInformation(getJwsParameter).ConfigureAwait(false);

//                Assert.True(result.JsonWebKey.ContainsKey("kid"));
//                Assert.Equal("RSA", (string)result.JsonWebKey.First().Value);
//            }
//        }

//        [Fact]
//        public async Task When_Extracting_Information_Of_Unsigned_Jws_Then_Information_Are_Returned()
//        {
//            InitializeFakeObjects();
//            var getJwsParameter = new GetJwsParameter
//            {
//                Jws = "jws"
//            };

//            var result = await _jwsActions.GetJwsInformation(getJwsParameter).ConfigureAwait(false);

//            Assert.NotNull(result);
//        }

//        [Fact]
//        public async Task When_Passing_Null_Parameter_To_GetPublicKeyInformation_Then_Exception_Is_Thrown()
//        {
//            InitializeFakeObjects();

//            await Assert.ThrowsAsync<ArgumentNullException>(() => _jwsActions.GetJwsInformation(null))
//                .ConfigureAwait(false);
//        }

//        [Fact]
//        public async Task When_Passing_Unsupported_Kty_To_GetPublicKeyInformation_Then_Exception_Is_Thrown()
//        {
//            var jsonWebKey = new JsonWebKey
//            {
//                Kty = JsonWebAlgorithmsKeyTypes.Octet //JwtConstants.KeyTypeValues.oct
//            };
//            InitializeFakeObjects(jsonWebKey);

//            var parameter = new GetJwsParameter
//            {
//                Url = new Uri("https://google.com"),
//                Jws = "jws"
//            };
//            var exception = await Assert
//                .ThrowsAsync<SimpleAuthException>(() => _jwsActions.GetJwsInformation(parameter))
//                .ConfigureAwait(false);
//            Assert.Equal(ErrorCodes.InvalidParameterCode, exception.Code);
//        }

//        [Fact]
//        public async Task When_Getting_Rsa_Key_Information_Then_Modulus_And_Exponent_Are_Returned()
//        {
//            using (var rsa = new RSACryptoServiceProvider())
//            {

//                var jsonWebKey = JsonWebKeyConverter.ConvertFromRSASecurityKey(new RsaSecurityKey(rsa));

//                InitializeFakeObjects(jsonWebKey);

//                var url = new Uri("https://blah");

//                var getJwsParameter = new GetJwsParameter
//                {
//                    Jws = "jws",
//                    Url = url
//                };
//                var result = await _jwsActions.GetJwsInformation(getJwsParameter).ConfigureAwait(false);

//                Assert.True(result.JsonWebKey.ContainsKey(JwtConstants.JsonWebKeyParameterNames.RsaKey.ModulusName));
//                Assert.True(result.JsonWebKey.ContainsKey(JwtConstants.JsonWebKeyParameterNames.RsaKey.ExponentName));
//            }
//        }

//        //[Fact]
//        //public void When_Passing_Null_Parameter_To_GetJsonWebKeyInformation_Then_Exception_Is_Thrown()
//        //{
//        //    InitializeFakeObjects();

//        //    Assert.Throws<ArgumentNullException>(() => _jsonWebKeyEnricher.GetJsonWebKeyInformation(null));
//        //}

//        [Fact]
//        public async Task When_Passing_Invalid_Kty_To_GetJsonWebKeyInformation_Then_Exception_Is_Thrown()
//        {
//            var jsonWebKey = new JsonWebKey
//            {
//                Kty = "200"
//            };

//            InitializeFakeObjects(jsonWebKey);

//            var parameter = new GetJwsParameter
//            {
//                Jws = "jws",
//                Url = new Uri("https://blah")
//            };
//            await Assert.ThrowsAsync<ArgumentException>(() => _jwsActions.GetJwsInformation(parameter))
//                .ConfigureAwait(false);
//        }

//        [Fact]
//        public async Task When_Passing_Invalid_Use_To_GetJsonWebKeyInformation_Then_Exception_Is_Thrown()
//        {
//            var jsonWebKey = new JsonWebKey
//            {
//                Kty = JwtConstants.KeyTypeValues.RSA,
//                Use = "200"
//            };

//            InitializeFakeObjects(jsonWebKey);

//            var parameter = new GetJwsParameter
//            {
//                Jws = "jws",
//                Url = new Uri("https://blah")
//            };
//            await Assert.ThrowsAsync<ArgumentException>(() => _jwsActions.GetJwsInformation(parameter))
//                .ConfigureAwait(false);
//        }

//        [Fact]
//        public async Task When_Passing_JsonWebKey_To_GetJsonWebKeyInformation_Then_Information_Are_Returned()
//        {
//            using (var rsa = new RSACryptoServiceProvider())
//            {
//                var jsonWebKey = JsonWebKeyConverter.ConvertFromSecurityKey(new RsaSecurityKey(rsa));
//                //    new JsonWebKey
//                //{
//                //    Kty = KeyType.RSA,
//                //    Use = Use.Sig,
//                //    Kid = "kid",
//                //    SerializedKey = serializedRsa
//                //};

//                InitializeFakeObjects(jsonWebKey);

//                var parameter = new GetJwsParameter
//                {
//                    Jws = "jws",
//                    Url = new Uri("https://blah")
//                };
//                var result = await _jwsActions.GetJwsInformation(parameter).ConfigureAwait(false);

//                Assert.True(result.JsonWebKey.ContainsKey(JwtConstants.JsonWebKeyParameterNames.KeyTypeName));
//                Assert.True(result.JsonWebKey.ContainsKey(JwtConstants.JsonWebKeyParameterNames.UseName));
//                Assert.True(result.JsonWebKey.ContainsKey(JwtConstants.JsonWebKeyParameterNames.AlgorithmName));
//                Assert.True(result.JsonWebKey.ContainsKey(JwtConstants.JsonWebKeyParameterNames.KeyIdentifierName));
//            }
//        }

//        private void InitializeFakeObjects(JsonWebKey jwk = null)
//        {
//            _jsonWebKeyHelperStub = new Mock<IJsonWebKeyHelper>();
//            if (jwk != null)
//            {
//                _jsonWebKeyHelperStub.Setup(x => x.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
//                    .ReturnsAsync(jwk);
//            }

//            _jwsActions = new JwsActions(
//                _jsonWebKeyHelperStub.Object);
//        }
//    }
//}
