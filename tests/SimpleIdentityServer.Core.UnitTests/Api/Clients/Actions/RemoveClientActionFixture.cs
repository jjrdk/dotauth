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
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Repositories;
    using Shared.Models;
    using Shared.Repositories;
    using Xunit;

    public class RemoveClientActionFixture
    {
        private IClientRepository _clientRepositoryStub;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _clientRepositoryStub.Delete(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Not_Existing_Client_Id_Then_ReturnsFalse()
        {
            const string clientId = "invalid_client_id";
            InitializeFakeObjects();

            var result = await _clientRepositoryStub.Delete(clientId).ConfigureAwait(false);

            Assert.False(result);
        }

        private void InitializeFakeObjects()
        {
            _clientRepositoryStub =
                new DefaultClientRepository(new Client[0], new HttpClient(), new DefaultScopeRepository());
        }
    }
}
