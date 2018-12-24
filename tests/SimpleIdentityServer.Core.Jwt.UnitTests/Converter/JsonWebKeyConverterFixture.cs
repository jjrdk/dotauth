// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Exceptions;
using SimpleIdentityServer.Core.Jwt.Serializer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Serialization;
using Xunit;

namespace SimpleIdentityServer.Core.Jwt.UnitTests.Converter
{
    using Json;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Requests;

    public sealed class JsonWebKeyConverterFixture
    {
        private IJsonWebKeyConverter _jsonWebKeyConverter;

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        Assert.Throws<ArgumentNullException>(() => _jsonWebKeyConverter.ExtractSerializedKeys(null));
        }

        [Fact]
        public void When_Passing_JsonWeb_Key_With_Missing_Kid_Then_An_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var jsonWebKeySet = new JsonWebKeySet
            {
                Keys = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        {
                            JwtConstants.JsonWebKeyParameterNames.KeyTypeName,
                            JwtConstants.KeyTypeValues.RsaName
                        }
                    }
                }
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<InvalidOperationException>(() => _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet));
            Assert.True(ex.Message == ErrorDescriptions.JwkIsInvalid);
        }

        [Fact]
        public void When_Passing_JsonWeb_Key_With_Not_Supported_Key_Type_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var jsonWebKeySet = new JsonWebKeySet
            {
                Keys = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        {
                            JwtConstants.JsonWebKeyParameterNames.KeyTypeName,
                            "not_supported"
                        },
                        {
                            JwtConstants.JsonWebKeyParameterNames.KeyIdentifierName,
                            "kid"
                        },
                        {
                            JwtConstants.JsonWebKeyParameterNames.UseName,
                            JwtConstants.UseValues.Encryption
                        }
                    }
                }
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<InvalidOperationException>(() => _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet));
            Assert.True(ex.Message == ErrorDescriptions.JwkIsInvalid);
        }

        [Fact]
        public void When_Passing_JsonWeb_Key_With_Not_Supported_Usage_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var jsonWebKeySet = new JsonWebKeySet
            {
                Keys = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        {
                            JwtConstants.JsonWebKeyParameterNames.KeyTypeName,
                            JwtConstants.KeyTypeValues.RsaName
                        },
                        {
                            JwtConstants.JsonWebKeyParameterNames.KeyIdentifierName,
                            "kid"
                        },
                        {
                            JwtConstants.JsonWebKeyParameterNames.UseName,
                            "invalid_usage"
                        }
                    }
                }
            };

            var json = jsonWebKeySet.SerializeWithJavascript();

            // ACT & ASSERTS
            var ex = Assert.Throws<InvalidOperationException>(() => _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet));
            Assert.True(ex.Message == ErrorDescriptions.JwkIsInvalid);
        }

        [Fact]
        public void When_Passing_JsonWeb_Key_Used_For_The_Signature_With_Rsa_Key_But_Wich_Doesnt_Contain_Modulus_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var jsonWebKey = new Dictionary<string, object>
            {
                {
                    JwtConstants.JsonWebKeyParameterNames.KeyTypeName,
                    JwtConstants.KeyTypeValues.RsaName
                },
                {
                    JwtConstants.JsonWebKeyParameterNames.KeyIdentifierName,
                    "kid"
                },
                {
                    JwtConstants.JsonWebKeyParameterNames.UseName,
                    JwtConstants.UseValues.Signature
                }
            };
            var jsonWebKeySet = new JsonWebKeySet
            {
                Keys = new List<Dictionary<string, object>>
                {
                    jsonWebKey
                }
            };
            var json = jsonWebKeySet.SerializeWithJavascript();

            // ACT & ASSERTS
            var ex = Assert.Throws<InvalidOperationException>(() => _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet));
            Assert.True(ex.Message == ErrorDescriptions.CannotExtractParametersFromJsonWebKey);
        }

        [Fact]
        public void When_Passing_JsonWeb_Key_Used_For_The_Signature_With_Ec_Key_But_Which_Doesnt_Contains_XCoordinate_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var jsonWebKey = new Dictionary<string, object>
            {
                {
                    JwtConstants.JsonWebKeyParameterNames.KeyTypeName,
                    JwtConstants.KeyTypeValues.EcName
                },
                {
                    JwtConstants.JsonWebKeyParameterNames.KeyIdentifierName,
                    "kid"
                },
                {
                    JwtConstants.JsonWebKeyParameterNames.UseName,
                    JwtConstants.UseValues.Signature
                }
            };
            var jsonWebKeySet = new JsonWebKeySet
            {
                Keys = new List<Dictionary<string, object>>
                {
                    jsonWebKey
                }
            };
            var json = jsonWebKeySet.SerializeWithJavascript();

            // ACT & ASSERTS
            var ex = Assert.Throws<InvalidOperationException>(() => _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet));
            Assert.True(ex.Message == ErrorDescriptions.CannotExtractParametersFromJsonWebKey);
        }

        [Fact]
        public void When_Passing_JsonWeb_Key_Used_For_The_Signature_With_Ec_Key_Which_Contains_Invalid_XCoordinate_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var jsonWebKey = new Dictionary<string, object>
            {
                {
                    JwtConstants.JsonWebKeyParameterNames.KeyTypeName,
                    JwtConstants.KeyTypeValues.EcName
                },
                {
                    JwtConstants.JsonWebKeyParameterNames.KeyIdentifierName,
                    "kid"
                },
                {
                    JwtConstants.JsonWebKeyParameterNames.UseName,
                    JwtConstants.UseValues.Signature
                },
                {
                    JwtConstants.JsonWebKeyParameterNames.EcKey.XCoordinateName,
                    "-%-!è'x_coordinate"
                },
                {
                    JwtConstants.JsonWebKeyParameterNames.EcKey.YCoordinateName,
                    "y_coordinate"
                }
            };
            var jsonWebKeySet = new JsonWebKeySet
            {
                Keys = new List<Dictionary<string, object>>
                {
                    jsonWebKey
                }
            };
            var json = jsonWebKeySet.SerializeWithJavascript();

            // ACT & ASSERTS
            var ex = Assert.Throws<InvalidOperationException>(() => _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet));
            Assert.True(ex.Message == ErrorDescriptions.OneOfTheParameterIsNotBase64Encoded);
        }

        [Fact]
        public void When_Passing_JsonWeb_Key_Used_For_The_Signature_With_Rsa_Key_Then_JsonWeb_Key_Is_Returned()
        {            InitializeFakeObjects();
            var jsonWebKey = new Dictionary<string, object>
            {
                {
                    JwtConstants.JsonWebKeyParameterNames.KeyTypeName,
                    JwtConstants.KeyTypeValues.RsaName
                },
                {
                    JwtConstants.JsonWebKeyParameterNames.KeyIdentifierName,
                    "kid"
                },
                {
                    JwtConstants.JsonWebKeyParameterNames.UseName,
                    JwtConstants.UseValues.Signature
                }
            };
            var jsonWebKeySet = new JsonWebKeySet
            {
                Keys = new List<Dictionary<string, object>>
                {
                    jsonWebKey
                }
            };

            using (var rsa = new RSACryptoServiceProvider())
            {
                var parameters = rsa.ExportParameters(false);

                jsonWebKey.Add(JwtConstants.JsonWebKeyParameterNames.RsaKey.ModulusName, parameters.Modulus.ToBase64Simplified());
                jsonWebKey.Add(JwtConstants.JsonWebKeyParameterNames.RsaKey.ExponentName, parameters.Exponent.ToBase64Simplified());

                var expectedXml = RsaExtensions.ToXmlString(rsa, false);

                // ACT & ASSERTS
                var result = _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet);
                Assert.Single(result);
                Assert.Equal(expectedXml, result.First().SerializedKey);
            }
        }

        [Fact]
        public void When_Passing_JsonWeb_Key_Used_For_The_Signature_With_Ec_Key_Then_JsonWeb_Key_Is_Returned()
        {            var xCoordinate = "x_coordinate".Base64Encode();
            var yCoordinate = "y_coordinate".Base64Encode();
            InitializeFakeObjects();
            var jsonWebKey = new Dictionary<string, object>
            {
                {
                    JwtConstants.JsonWebKeyParameterNames.KeyTypeName,
                    JwtConstants.KeyTypeValues.EcName
                },
                {
                    JwtConstants.JsonWebKeyParameterNames.KeyIdentifierName,
                    "kid"
                },
                {
                    JwtConstants.JsonWebKeyParameterNames.UseName,
                    JwtConstants.UseValues.Signature
                },
                {
                    JwtConstants.JsonWebKeyParameterNames.EcKey.XCoordinateName,
                    xCoordinate
                },
                {
                    JwtConstants.JsonWebKeyParameterNames.EcKey.YCoordinateName,
                    yCoordinate
                }
            };
            var jsonWebKeySet = new JsonWebKeySet
            {
                Keys = new List<Dictionary<string, object>>
                {
                    jsonWebKey
                }
            };
            var cngKeySerialized = new CngKeySerialized
            {
                X = xCoordinate.Base64DecodeBytes(),
                Y = yCoordinate.Base64DecodeBytes()
            };
            var serializer = new XmlSerializer(typeof(CngKeySerialized));
            var serializedKeys = string.Empty;
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, cngKeySerialized);
                serializedKeys = writer.ToString();
            }

            // ACT & ASSERTS
            var result = _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet);
            Assert.NotNull(result);
            Assert.True(result.Count() == 1);
            Assert.True(result.First().SerializedKey == serializedKeys);
        }

        private void InitializeFakeObjects()
        {
            _jsonWebKeyConverter = new JsonWebKeyConverter();
        }
    }
}