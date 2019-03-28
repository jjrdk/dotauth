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

namespace SimpleAuth.Tests.Validators
{
    using Shared;
    using Shared.Models;
    using SimpleAuth.Extensions;
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using Xunit;

    public class ClientValidatorFixture
    {
        [Fact]
        public void When_Client_Does_Not_Contain_RedirectionUri_Then_EmptyArray_Is_Returned()
        {
            Assert.Empty(((Client)null).GetRedirectionUrls(null, null));
            Assert.Empty(new Client().GetRedirectionUrls(null));
            Assert.Empty(new Client().GetRedirectionUrls(new Uri("https://url")));
            Assert.Empty(new Client().GetRedirectionUrls(new Uri("https://url")));
        }

        [Fact]
        public void When_Checking_RedirectionUri_Then_Uri_Is_Returned()
        {
            var url = new Uri("https://url/");
            var client = new Client { RedirectionUrls = new[] { url } };

            var result = client.GetRedirectionUrls(url);

            Assert.Equal(url, result.First());
        }

        [Fact]
        public void When_Passing_Null_Parameter_To_ValidateGrantType_Then_False_Is_Returned()
        {
            var result = ((Client)null).CheckGrantTypes(GrantTypes.AuthorizationCode);

            Assert.False(result);
        }

        [Fact]
        public void When_Client_Does_Not_Have_GrantType_Then_AuthorizationCode_Is_Assigned()
        {
            var client = new Client();

            var result = client.CheckGrantTypes(GrantTypes.AuthorizationCode);

            Assert.True(result);
            Assert.Contains(GrantTypes.AuthorizationCode, client.GrantTypes);
        }

        [Fact]
        public void When_Checking_Client_Has_Implicit_Grant_Type_Then_True_Is_Returned()
        {
            var client = new Client { GrantTypes = new[] { GrantTypes.Implicit } };

            var result = client.CheckGrantTypes(GrantTypes.Implicit);

            Assert.True(result);
        }

        [Fact]
        public void When_Passing_Null_Client_Then_False_Is_Returned()
        {
            Assert.False(((Client)null).CheckGrantTypes(null, null));
        }

        [Fact]
        public void When_Passing_Null_GrantTypes_Then_True_Is_Returned()
        {
            Assert.True(new Client().CheckGrantTypes(null));
        }

        [Fact]
        public void When_Checking_Client_Grant_Types_Then_True_Is_Returned()
        {
            var client = new Client { GrantTypes = new[] { GrantTypes.Implicit, GrantTypes.Password } };


            Assert.True(client.CheckGrantTypes(GrantTypes.Implicit, GrantTypes.Password));
            Assert.True(client.CheckGrantTypes(GrantTypes.Implicit));
        }

        [Fact]
        public void When_Checking_Client_Grant_Types_Then_False_Is_Returned()
        {
            var client = new Client { GrantTypes = new[] { GrantTypes.Implicit, GrantTypes.Password } };

            Assert.False(client.CheckGrantTypes(GrantTypes.RefreshToken));
            Assert.False(client.CheckGrantTypes(GrantTypes.RefreshToken, GrantTypes.Password));
        }

        [Fact]
        public void WhenPassingNullNullClientThenThrows()
        {
            Assert.Throws<NullReferenceException>(() => ((Client)null).CheckPkce(null, null));
        }

        [Fact]
        public void WhenPassingNullParametersThenReturnsFalse()
        {
            Assert.Throws<NullReferenceException>(() => new Client {RequirePkce = true}.CheckPkce(null, null));
        }

        [Fact]
        public void When_RequirePkce_Is_False_Then_True_Is_Returned()
        {
            var result = new Client { RequirePkce = false }.CheckPkce(null, new AuthorizationCode());

            Assert.True(result);
        }

        [Fact]
        public void When_Plain_CodeChallenge_Is_Not_Correct_Then_False_Is_Returned()
        {
            var result = new Client { RequirePkce = true }.CheckPkce(
                "invalid_code",
                new AuthorizationCode { CodeChallenge = "code", CodeChallengeMethod = CodeChallengeMethods.Plain });

            Assert.False(result);
        }

        [Fact]
        public void When_RS256_CodeChallenge_Is_Not_Correct_Then_False_Is_Returned()
        {
            var result = new Client { RequirePkce = true }.CheckPkce(
                "code",
                new AuthorizationCode { CodeChallenge = "code", CodeChallengeMethod = CodeChallengeMethods.Rs256 });

            Assert.False(result);
        }

        [Fact]
        public void When_RS256_CodeChallenge_Is_Correct_Then_True_Is_Returned()
        {
            var hashed = SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes("code"));
            var codeChallenge = hashed.ToBase64Simplified();

            var result = new Client { RequirePkce = true }.CheckPkce(
                "code",
                new AuthorizationCode
                {
                    CodeChallenge = codeChallenge,
                    CodeChallengeMethod = CodeChallengeMethods.Rs256
                });

            Assert.True(result);
        }
    }
}
