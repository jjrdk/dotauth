﻿// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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
    using System.Threading;
    using System.Threading.Tasks;
    using Repositories;
    using Shared.Models;
    using Shared.Repositories;
    using Xunit;

    public class RemoveClientActionFixture
    {
        private readonly IClientRepository _clientRepositoryStub;

        public RemoveClientActionFixture()
        {
            _clientRepositoryStub = new InMemoryClientRepository(
                new HttpClient(),
                new InMemoryScopeRepository(),
                new Client[0]);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _clientRepositoryStub.Delete(null, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Not_Existing_Client_Id_Then_ReturnsFalse()
        {
            const string clientId = "invalid_client_id";

            var result = await _clientRepositoryStub.Delete(clientId, CancellationToken.None).ConfigureAwait(false);

            Assert.False(result);
        }
    }
}