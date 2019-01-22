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
    using Parameters;
    using SimpleAuth.Services;
    using SimpleAuth.WebSite.Authenticate;
    using SimpleAuth.WebSite.Authenticate.Actions;
    using SimpleAuth.WebSite.Authenticate.Common;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class AuthenticateActionsFixture
    {
        private Mock<IAuthenticateResourceOwnerOpenIdAction> _authenticateResourceOwnerActionFake;
        private Mock<IGenerateAndSendCodeAction> _generateAndSendCodeActionStub;
        private Mock<IValidateConfirmationCodeAction> _validateConfirmationCodeActionStub;
        private Mock<IRemoveConfirmationCodeAction> _removeConfirmationCodeActionStub;
        private IAuthenticateActions _authenticateActions;

        [Fact]
        public async Task
            When_Passing_Null_AuthorizationParameter_To_The_Action_AuthenticateResourceOwner_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();

            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _authenticateActions.AuthenticateResourceOwnerOpenId(null, null, null, null))
                .ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _authenticateActions.AuthenticateResourceOwnerOpenId(
                        authorizationParameter,
                        null,
                        null,
                        null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task
            When_Passing_Null_LocalAuthenticateParameter_To_The_Action_LocalUserAuthentication_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var localAuthenticationParameter = new LocalAuthenticationParameter();
            var authentication = new LocalOpenIdUserAuthenticationAction(new IAuthenticateResourceOwnerService[0], new Mock<IAuthenticateHelper>().Object);
            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => authentication.Execute(null, null, null, null))
                .ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => authentication.Execute(
                        localAuthenticationParameter,
                        null,
                        null,
                        null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task
            When_Passing_Parameters_Needed_To_The_Action_AuthenticateResourceOwner_Then_The_Action_Is_Called()
        {
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter { ClientId = "clientId" };
            var claimsPrincipal = new ClaimsPrincipal();

            await _authenticateActions
                .AuthenticateResourceOwnerOpenId(authorizationParameter, claimsPrincipal, null, null)
                .ConfigureAwait(false);

            _authenticateResourceOwnerActionFake.Verify(
                a => a.Execute(authorizationParameter, claimsPrincipal, null, null));
        }

        private void InitializeFakeObjects()
        {
            _authenticateResourceOwnerActionFake = new Mock<IAuthenticateResourceOwnerOpenIdAction>();
            _generateAndSendCodeActionStub = new Mock<IGenerateAndSendCodeAction>();
            _validateConfirmationCodeActionStub = new Mock<IValidateConfirmationCodeAction>();
            _removeConfirmationCodeActionStub = new Mock<IRemoveConfirmationCodeAction>();
            _authenticateActions = new AuthenticateActions(
                _authenticateResourceOwnerActionFake.Object,
                _generateAndSendCodeActionStub.Object,
                _validateConfirmationCodeActionStub.Object,
                _removeConfirmationCodeActionStub.Object);
        }
    }
}
