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

namespace SimpleAuth.Tests.Api.Clients.Actions
{
    using Exceptions;
    using Helpers;
    using Moq;
    using Newtonsoft.Json;
    using Repositories;
    using Shared.Models;
    using Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;

    public class UpdateClientActionFixture
    {
        private IClientRepository _clientRepositoryMock;
        private Mock<IScopeRepository> _scopeRepositoryStub;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _clientRepositoryMock.Update(null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_No_Client_Id_Is_Passed_Then_ReturnsNull()
        {
            InitializeFakeObjects();
            var parameter = new Client
            {
                ClientId = null
            };

            var result = await _clientRepositoryMock.Update(parameter).ConfigureAwait(false);

            Assert.Null(result);
        }

        [Fact]
        public async Task When_Client_Does_Not_Exist_Then_ReturnsNull()
        {
            const string clientId = "invalid_client_id";
            InitializeFakeObjects();

            var parameter = new Client
            {
                ClientId = clientId,
                RedirectionUrls = new List<Uri> { new Uri("https://localhost") },
                Secrets = new List<ClientSecret>
                {
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.SharedSecret,
                        Value = "test"
                    }
                }
            };

            var jsonParameter = JsonConvert.SerializeObject(parameter);
            var result = await _clientRepositoryMock.Update(parameter).ConfigureAwait(false);

            Assert.Null(result);
        }

        [Fact]
        public async Task When_Scope_Are_Not_Supported_Then_Exception_Is_Thrown()
        {
            const string clientId = "client_id";
            var client = new Client
            {
                ClientId = clientId
            };
            var parameter = new Client
            {
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                ClientId = clientId,
                AllowedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "not_supported_scope"
                    }
                },
                RequestUris = new[] { new Uri("https://localhost"), },
                RedirectionUrls = new[] { new Uri("https://localhost") }
            };
            InitializeFakeObjects(new[] { client });

            _scopeRepositoryStub.Setup(s => s.SearchByNames(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult((ICollection<Scope>)new List<Scope>
                {
                    new Scope
                    {
                        Name = "scope"
                    }
                }));

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _clientRepositoryMock.Update(parameter))
                .ConfigureAwait(false);

            Assert.Equal("Unknown scopes: not_supported_scope", ex.Message);
        }

        private void InitializeFakeObjects(IReadOnlyCollection<Client> clients = null)
        {
            _clientRepositoryMock = new DefaultClientRepository(clients ?? new Client[0],
                new HttpClient(),
                new DefaultScopeRepository());
            _scopeRepositoryStub = new Mock<IScopeRepository>();
        }
    }
}
