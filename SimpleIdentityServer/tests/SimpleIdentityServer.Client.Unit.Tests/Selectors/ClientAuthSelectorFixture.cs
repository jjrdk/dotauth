﻿#region copyright
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
#endregion

using Moq;
using SimpleIdentityServer.Client.Builders;
using SimpleIdentityServer.Client.DTOs.Request;
using SimpleIdentityServer.Client.Selectors;
using SimpleIdentityServer.Core.Common.Extensions;
using System;
using Xunit;

namespace SimpleIdentityServer.Client.Unit.Tests.Selectors
{
    public class ClientAuthSelectorFixture
    {
        private Mock<ITokenRequestBuilder> _tokenRequestBuilderStub;

        private Mock<ITokenGrantTypeSelector> _tokenGrantTypeSelectorStub;

        private IClientAuthSelector _clientAuthSelector;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameters_To_ClientSecretBasic_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _clientAuthSelector.UseClientSecretBasicAuth(null, null));
            Assert.Throws<ArgumentNullException>(() => _clientAuthSelector.UseClientSecretBasicAuth("client_id", null));
        }

        [Fact]
        public void When_Passing_Null_Parameters_To_ClientSecretPost_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _clientAuthSelector.UseClientSecretPostAuth(null, null));
            Assert.Throws<ArgumentNullException>(() => _clientAuthSelector.UseClientSecretPostAuth("client_id", null));
        }

        #endregion

        #region Happy paths

        [Fact]
        public void When_Using_ClientSecretAuth_Then_Selector_Is_Returned()
        {
            // ARRANGE
            const string clientId = "client_id";
            const string clientSecret = "client_secret";
            var token = (clientId + ":" + clientSecret).Base64Encode();
            InitializeFakeObjects();
            _tokenRequestBuilderStub.Setup(s => s.AuthorizationHeaderValue).Returns(string.Empty);

            // ACT
            var result = _clientAuthSelector.UseClientSecretBasicAuth(clientId, clientSecret);

            // ASSERT
            Assert.NotNull(result);
            _tokenRequestBuilderStub.VerifySet(t => t.AuthorizationHeaderValue = token);
        }

        [Fact]
        public void When_Using_ClientSecretPost_Then_Selector_Is_Returned()
        {
            // ARRANGE
            const string clientId = "client_id";
            const string clientSecret = "client_secret";
            var tokenRequest = new TokenRequest();
            InitializeFakeObjects();
            _tokenRequestBuilderStub.Setup(s => s.TokenRequest).Returns(tokenRequest);

            // ACT
            var result = _clientAuthSelector.UseClientSecretPostAuth(clientId, clientSecret);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(tokenRequest.ClientId == clientId);
            Assert.True(tokenRequest.ClientSecret == clientSecret);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _tokenRequestBuilderStub = new Mock<ITokenRequestBuilder>();
            _tokenGrantTypeSelectorStub = new Mock<ITokenGrantTypeSelector>();
            _clientAuthSelector = new ClientAuthSelector(_tokenRequestBuilderStub.Object, _tokenGrantTypeSelectorStub.Object);
        }
    }
}
