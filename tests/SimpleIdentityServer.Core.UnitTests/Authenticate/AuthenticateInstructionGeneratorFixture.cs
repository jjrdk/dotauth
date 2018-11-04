// Copyright 2015 Habart Thierry
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

using SimpleIdentityServer.Core.Authenticate;
using System.Net.Http.Headers;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Authenticate
{
    using Shared;

    public class AuthenticateInstructionGeneratorFixture
    {
        [Fact]
        public void When_Passing_No_Parameter_Then_Empty_Result_Is_Returned()
        {
            // ACT
            var header = (AuthenticationHeaderValue)null;
            var result = header.GetAuthenticateInstruction(null);

            // ASSERT
            Assert.True(string.IsNullOrWhiteSpace(result.ClientIdFromAuthorizationHeader));
            Assert.True(string.IsNullOrWhiteSpace(result.ClientSecretFromAuthorizationHeader));
        }

        [Fact]
        public void When_Passing_Empty_AuthenticationHeaderParameter_Then_Empty_Result_Is_Returned()
        {
            var authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", string.Empty);

            // ACT
            var result = authenticationHeaderValue.GetAuthenticateInstruction(null);

            // ASSERT
            Assert.True(string.IsNullOrWhiteSpace(result.ClientIdFromAuthorizationHeader));
            Assert.True(string.IsNullOrWhiteSpace(result.ClientSecretFromAuthorizationHeader));
        }

        [Fact]
        public void When_Passing_Not_Valid_Parameter_Then_Empty_Result_Is_Returned()
        {
            var parameter = "parameter";
            var encodedParameter = parameter.Base64Encode();
            var authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", encodedParameter);

            // ACT
            var result = authenticationHeaderValue.GetAuthenticateInstruction(null);

            // ASSERT
            Assert.True(string.IsNullOrWhiteSpace(result.ClientIdFromAuthorizationHeader));
            Assert.True(string.IsNullOrWhiteSpace(result.ClientSecretFromAuthorizationHeader));
        }

        [Fact]
        public void When_Passing_Valid_Parameter_Then_Valid_AuthenticateInstruction_Is_Returned()
        {
            const string clientId = "clientId";
            const string clientSecret = "clientSecret";
            var parameter = string.Format("{0}:{1}", clientId, clientSecret);
            var encodedParameter = parameter.Base64Encode();
            var authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", encodedParameter);

            // ACT
            var result = authenticationHeaderValue.GetAuthenticateInstruction(null);

            // ASSERT
            Assert.True(result.ClientIdFromAuthorizationHeader == clientId);
            Assert.True(result.ClientSecretFromAuthorizationHeader == clientSecret);
        }
    }
}
