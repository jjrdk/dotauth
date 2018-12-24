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

using SimpleIdentityServer.Core.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Validators
{
    using Shared;
    using Shared.Models;

    public class ClientValidatorFixture
    {
        private IClientValidator _clientValidator;

        [Fact]
        public void When_Client_Doesnt_Contain_RedirectionUri_Then_EmptyArray_Is_Returned()
        {
            InitializeMockingObjects();

            // ACTS & ASSERTS
            Assert.Empty(_clientValidator.GetRedirectionUrls(null, null));
            Assert.Empty(_clientValidator.GetRedirectionUrls(new Client(), null));
            Assert.Empty(_clientValidator.GetRedirectionUrls(new Client(), new Uri("https://url")));
            Assert.Empty(_clientValidator.GetRedirectionUrls(new Client
                {
                    RedirectionUrls = new List<Uri>()
                },
                new Uri("https://url")));
        }

        [Fact]
        public void When_Checking_RedirectionUri_Then_Uri_Is_Returned()
        {
            var url = new Uri("https://url/");
            var client = new Client
            {
                RedirectionUrls = new List<Uri>
                {
                    url
                }
            };
            InitializeMockingObjects();

            var result = _clientValidator.GetRedirectionUrls(client, url);

            Assert.Equal(url, result.First());
        }

        [Fact]
        public void When_Passing_Null_Parameter_To_ValidateGrantType_Then_False_Is_Returned()
        {
            InitializeMockingObjects();

            var result = _clientValidator.CheckGrantTypes(null, GrantType.authorization_code);

            Assert.False(result);
        }

        [Fact]
        public void When_Client_Doesnt_Have_GrantType_Then_AuthorizationCode_Is_Assigned()
        {
            InitializeMockingObjects();
            var client = new Client();

            var result = _clientValidator.CheckGrantTypes(client, GrantType.authorization_code);

            Assert.True(result);
            Assert.Contains(GrantType.authorization_code, client.GrantTypes);
        }

        [Fact]
        public void When_Checking_Client_Has_Implicit_Grant_Type_Then_True_Is_Returned()
        {
            InitializeMockingObjects();
            var client = new Client
            {
                GrantTypes = new List<GrantType>
                {
                    GrantType.@implicit
                }
            };

            var result = _clientValidator.CheckGrantTypes(client, GrantType.@implicit);

            Assert.True(result);
        }

        [Fact]
        public void When_Passing_Null_Client_Then_False_Is_Returned()
        {
            InitializeMockingObjects();

            // ACTS & ASSERTS
            Assert.False(_clientValidator.CheckGrantTypes(null, null));
        }

        [Fact]
        public void When_Passing_Null_GrantTypes_Then_True_Is_Returned()
        {
            InitializeMockingObjects();

            // ACTS & ASSERTS
            Assert.True(_clientValidator.CheckGrantTypes(new Client(), null));
        }

        [Fact]
        public void When_Checking_Client_Grant_Types_Then_True_Is_Returned()
        {
            InitializeMockingObjects();
            var client = new Client
            {
                GrantTypes = new List<GrantType>
                {
                    GrantType.@implicit,
                    GrantType.password
                }
            };

            // ACTS & ASSERTS
            Assert.True(_clientValidator.CheckGrantTypes(client, GrantType.@implicit, GrantType.password));
            Assert.True(_clientValidator.CheckGrantTypes(client, GrantType.@implicit));
        }

        [Fact]
        public void When_Checking_Client_Grant_Types_Then_False_Is_Returned()
        {
            InitializeMockingObjects();
            var client = new Client
            {
                GrantTypes = new List<GrantType>
                {
                    GrantType.@implicit,
                    GrantType.password
                }
            };

            // ACTS & ASSERTS
            Assert.False(_clientValidator.CheckGrantTypes(client, GrantType.refresh_token));
            Assert.False(_clientValidator.CheckGrantTypes(client, GrantType.refresh_token, GrantType.password));
        }

        [Fact]
        public void When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            InitializeMockingObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _clientValidator.CheckPkce(null, null, null));
            Assert.Throws<ArgumentNullException>(() => _clientValidator.CheckPkce(new Client(), null, null));
        }

        [Fact]
        public void When_RequirePkce_Is_False_Then_True_Is_Returned()
        {
            InitializeMockingObjects();

            var result = _clientValidator.CheckPkce(new Client
            {
                RequirePkce = false
            }, null, new AuthorizationCode());

            Assert.True(result);

        }

        [Fact]
        public void When_Plain_CodeChallenge_Is_Not_Correct_Then_False_Is_Returned()
        {
            InitializeMockingObjects();

            var result = _clientValidator.CheckPkce(new Client
            {
                RequirePkce = true
            }, "invalid_code", new AuthorizationCode
            {
                CodeChallenge = "code",
                CodeChallengeMethod = CodeChallengeMethods.Plain
            });

            Assert.False(result);
        }

        [Fact]
        public void When_RS256_CodeChallenge_Is_Not_Correct_Then_False_Is_Returned()
        {
            InitializeMockingObjects();

            var result = _clientValidator.CheckPkce(new Client
            {
                RequirePkce = true
            }, "code", new AuthorizationCode
            {
                CodeChallenge = "code",
                CodeChallengeMethod = CodeChallengeMethods.RS256
            });

            Assert.False(result);
        }

        [Fact]
        public void When_RS256_CodeChallenge_Is_Correct_Then_True_Is_Returned()
        {
            InitializeMockingObjects();
            var hashed = SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes("code"));
            var codeChallenge = hashed.ToBase64Simplified();

            var result = _clientValidator.CheckPkce(new Client
            {
                RequirePkce = true
            }, "code", new AuthorizationCode
            {
                CodeChallenge = codeChallenge,
                CodeChallengeMethod = CodeChallengeMethods.RS256
            });

            Assert.True(result);
        }

        private void InitializeMockingObjects()
        {
            _clientValidator = new ClientValidator();
        }
    }
}
