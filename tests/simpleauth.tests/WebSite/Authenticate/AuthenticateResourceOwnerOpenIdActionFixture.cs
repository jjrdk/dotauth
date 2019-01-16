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

namespace SimpleAuth.Tests.WebSite.Authenticate
{
    using Moq;
    using Newtonsoft.Json;
    using Parameters;
    using Results;
    using SimpleAuth.Helpers;
    using SimpleAuth.WebSite.Authenticate.Actions;
    using SimpleAuth.WebSite.Authenticate.Common;
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Shared;
    using Xunit;

    public sealed class AuthenticateResourceOwnerOpenIdActionFixture
    {
        private Mock<IParameterParserHelper> _parameterParserHelperFake;
        private Mock<IAuthenticateHelper> _authenticateHelperFake;
        private IAuthenticateResourceOwnerOpenIdAction _authenticateResourceOwnerOpenIdAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert
                .ThrowsAsync<ArgumentNullException>(() =>
                    _authenticateResourceOwnerOpenIdAction.Execute(null, null, null, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_No_Resource_Owner_Is_Passed_Then_Redirect_To_Index_Page()
        {
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();

            var result = await _authenticateResourceOwnerOpenIdAction.Execute(authorizationParameter, null, null, null)
                .ConfigureAwait(false);

            Assert.Equal(JsonConvert.SerializeObject(
                    EndpointResult.CreateAnEmptyActionResultWithNoEffect()),
                JsonConvert.SerializeObject(result));
        }

        [Fact]
        public async Task When_Resource_Owner_Is_Not_Authenticated_Then_Redirect_To_Index_Page()
        {
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var result = await _authenticateResourceOwnerOpenIdAction.Execute(authorizationParameter,
                    claimsPrincipal,
                    null,
                    null)
                .ConfigureAwait(false);

            Assert.Equal(
                JsonConvert.SerializeObject(EndpointResult.CreateAnEmptyActionResultWithNoEffect()),
                JsonConvert.SerializeObject(result));
        }

        [Fact]
        public async Task When_Prompt_Parameter_Contains_Login_Value_Then_Redirect_To_Index_Page()
        {
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();
            var claimsIdentity = new ClaimsIdentity("authServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var promptParameters = new List<PromptParameter>
            {
                PromptParameter.login
            };
            _parameterParserHelperFake.Setup(p => p.ParsePrompts(It.IsAny<string>()))
                .Returns(promptParameters);

            var result = await _authenticateResourceOwnerOpenIdAction.Execute(authorizationParameter,
                    claimsPrincipal,
                    null,
                    null)
                .ConfigureAwait(false);

            Assert.Equal(
                JsonConvert.SerializeObject(EndpointResult.CreateAnEmptyActionResultWithNoEffect()),
                JsonConvert.SerializeObject(result));
        }

        [Fact]
        public async Task
            When_Prompt_Parameter_Does_Not_Contain_Login_Value_And_Resource_Owner_Is_Authenticated_Then_Helper_Is_Called()
        {
            InitializeFakeObjects();
            const string code = "code";
            const string subject = "subject";
            var authorizationParameter = new AuthorizationParameter();
            var claims = new List<Claim>
            {
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimsIdentity = new ClaimsIdentity(claims, "authServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var promptParameters = new List<PromptParameter>
            {
                PromptParameter.consent
            };
            _parameterParserHelperFake.Setup(p => p.ParsePrompts(It.IsAny<string>()))
                .Returns(promptParameters);

            await _authenticateResourceOwnerOpenIdAction.Execute(
                    authorizationParameter,
                    claimsPrincipal,
                    code,
                    null)
                .ConfigureAwait(false);

            _authenticateHelperFake.Verify(a => a.ProcessRedirection(authorizationParameter,
                code,
                subject,
                It.IsAny<List<Claim>>(),
                null));
        }

        private void InitializeFakeObjects()
        {
            _parameterParserHelperFake = new Mock<IParameterParserHelper>();
            _authenticateHelperFake = new Mock<IAuthenticateHelper>();
            _authenticateResourceOwnerOpenIdAction = new AuthenticateResourceOwnerOpenIdAction(
                _parameterParserHelperFake.Object,
                _authenticateHelperFake.Object);
        }
    }
}
