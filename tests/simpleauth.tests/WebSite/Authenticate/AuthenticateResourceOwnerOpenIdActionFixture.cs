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
    using Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.WebSite.Authenticate;
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Divergic.Logging.Xunit;
    using SimpleAuth.Repositories;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class AuthenticateResourceOwnerOpenIdActionFixture
    {
        private readonly AuthenticateResourceOwnerOpenIdAction _authenticateResourceOwnerOpenIdAction;

        public AuthenticateResourceOwnerOpenIdActionFixture(ITestOutputHelper outputHelper)
        {
            var mock = new Mock<IClientStore>();
            mock.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Client());
            _authenticateResourceOwnerOpenIdAction = new AuthenticateResourceOwnerOpenIdAction(
                new Mock<IAuthorizationCodeStore>().Object,
                new Mock<ITokenStore>().Object,
                new Mock<IScopeRepository>().Object,
                new Mock<IConsentRepository>().Object,
                mock.Object,
                new InMemoryJwksRepository(),
                new NoOpPublisher(),
                new TestOutputLogger("test", outputHelper));
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            await Assert
                .ThrowsAsync<NullReferenceException>(
                    () => _authenticateResourceOwnerOpenIdAction.Execute(null, null, null, null, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_No_Resource_Owner_Is_Passed_Then_Redirect_To_Index_Page()
        {
            var authorizationParameter = new AuthorizationParameter();

            var result = await _authenticateResourceOwnerOpenIdAction.Execute(authorizationParameter, null, null, null, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Equal(
                JsonConvert.SerializeObject(EndpointResult.CreateAnEmptyActionResultWithNoEffect()),
                JsonConvert.SerializeObject(result));
        }

        [Fact]
        public async Task When_Resource_Owner_Is_Not_Authenticated_Then_Redirect_To_Index_Page()
        {
            var authorizationParameter = new AuthorizationParameter();
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var result = await _authenticateResourceOwnerOpenIdAction
                .Execute(authorizationParameter, claimsPrincipal, null, null, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Equal(
                JsonConvert.SerializeObject(EndpointResult.CreateAnEmptyActionResultWithNoEffect()),
                JsonConvert.SerializeObject(result));
        }

        [Fact]
        public async Task When_Prompt_Parameter_Contains_Login_Value_Then_Redirect_To_Index_Page()
        {
            var authorizationParameter = new AuthorizationParameter
            {
                Prompt = "login",
                ClientId = "client",
                Scope = "scope"
            };
            var claimsIdentity = new ClaimsIdentity("authServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var result = await _authenticateResourceOwnerOpenIdAction.Execute(
                    authorizationParameter,
                    claimsPrincipal,
                    null,
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Equal(
                JsonConvert.SerializeObject(EndpointResult.CreateAnEmptyActionResultWithNoEffect()),
                JsonConvert.SerializeObject(result));
        }

        [Fact]
        public async Task
            When_Prompt_Parameter_Does_Not_Contain_Login_Value_And_Resource_Owner_Is_Authenticated_Then_Helper_Is_Called()
        {
            const string code = "code";
            const string subject = "subject";
            var authorizationParameter = new AuthorizationParameter { ClientId = "abc" };
            var claims = new List<Claim> { new(OpenIdClaimTypes.Subject, subject) };
            var claimsIdentity = new ClaimsIdentity(claims, "authServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var result = await _authenticateResourceOwnerOpenIdAction.Execute(
                    authorizationParameter,
                    claimsPrincipal,
                    code,
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.NotNull(result.RedirectInstruction);
        }
    }
}
