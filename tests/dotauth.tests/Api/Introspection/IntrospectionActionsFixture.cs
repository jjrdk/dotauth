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

namespace DotAuth.Tests.Api.Introspection;

using System;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.Introspection;
using DotAuth.Repositories;
using Xunit;

public sealed class IntrospectionActionsFixture
{
    private readonly PostIntrospectionAction _introspectionActions;

    public IntrospectionActionsFixture()
    {
        _introspectionActions = new PostIntrospectionAction(
            new InMemoryTokenStore());
    }

    [Fact]
    public async Task When_Passing_Null_Parameter_To_PostIntrospection_Then_Exception_Is_Thrown()
    {
        await Assert
            .ThrowsAsync<NullReferenceException>(
                () => _introspectionActions.Execute(null, CancellationToken.None))
            .ConfigureAwait(false);
    }
}