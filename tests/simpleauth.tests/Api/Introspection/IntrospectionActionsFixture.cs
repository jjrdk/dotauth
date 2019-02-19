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

namespace SimpleAuth.Tests.Api.Introspection
{
    using Repositories;
    using Shared.Models;
    using SimpleAuth.Api.Introspection;
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class IntrospectionActionsFixture
    {
        private readonly PostIntrospectionAction _introspectionActions;

        public IntrospectionActionsFixture()
        {
            _introspectionActions = new PostIntrospectionAction(
                new InMemoryClientRepository(
                    new HttpClient(),
                    new InMemoryScopeRepository(new Scope[0]),
                    new Client[0]),
                new InMemoryTokenStore());
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_To_PostIntrospection_Then_Exception_Is_Thrown()
        {
            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _introspectionActions.Execute(null, null, null, CancellationToken.None))
                .ConfigureAwait(false);
        }
    }
}
