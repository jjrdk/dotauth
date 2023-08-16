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

namespace DotAuth.Tests.TwoFactors;

using System;
using System.Threading.Tasks;
using DotAuth.Services;
using NSubstitute;
using Xunit;

public sealed class TwoFactorAuthenticationHandlerFixture
{
    private readonly ITwoFactorAuthenticationHandler _twoFactorAuthenticationHandler;

    public TwoFactorAuthenticationHandlerFixture()
    {
        _twoFactorAuthenticationHandler = new TwoFactorAuthenticationHandler(new[] { Substitute.For<ITwoFactorAuthenticationService>(), });
    }

    [Fact]
    public async Task When_Passing_Null_Parameter_To_SendCode_Then_Exception_Is_Thrown()
    {
        await Assert
            .ThrowsAsync<ArgumentNullException>(() => _twoFactorAuthenticationHandler.SendCode(null, null, null))
            .ConfigureAwait(false);
        await Assert
            .ThrowsAsync<ArgumentNullException>(
                () => _twoFactorAuthenticationHandler.SendCode(string.Empty, null, null))
            .ConfigureAwait(false);
        await Assert
            .ThrowsAsync<ArgumentNullException>(() => _twoFactorAuthenticationHandler.SendCode("code", null, null))
            .ConfigureAwait(false);
        await Assert
            .ThrowsAsync<ArgumentNullException>(
                () => _twoFactorAuthenticationHandler.SendCode("code", string.Empty, null))
            .ConfigureAwait(false);
    }
}
