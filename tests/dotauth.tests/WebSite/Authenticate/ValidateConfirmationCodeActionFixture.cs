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

namespace DotAuth.Tests.WebSite.Authenticate;

using System;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.WebSite.Authenticate;
using NSubstitute;
using Xunit;

public sealed class ValidateConfirmationCodeActionFixture
{
    private readonly IConfirmationCodeStore _confirmationCodeStoreStub;
    private readonly ValidateConfirmationCodeAction _validateConfirmationCodeAction;

    public ValidateConfirmationCodeActionFixture()
    {
        _confirmationCodeStoreStub = Substitute.For<IConfirmationCodeStore>();
        _validateConfirmationCodeAction = new ValidateConfirmationCodeAction(_confirmationCodeStoreStub);
    }

    [Fact]
    public async Task When_Passing_Null_Parameter_Then_Returns_False()
    {
        var result = await _validateConfirmationCodeAction.Execute(null, null, CancellationToken.None)
            ;

        Assert.False(result);
    }

    [Fact]
    public async Task When_Passing_Empty_Parameter_Then_Returns_False()
    {
        var result = await _validateConfirmationCodeAction.Execute(string.Empty, string.Empty, CancellationToken.None)
            ;

        Assert.False(result);
    }


    [Fact]
    public async Task When_Code_Does_Not_Exist_Then_False_Is_Returned()
    {
        _confirmationCodeStoreStub.Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ConfirmationCode)null);

        var result = await _validateConfirmationCodeAction.Execute("code", "test", CancellationToken.None)
            ;
        Assert.False(result);
    }

    [Fact]
    public async Task When_Code_Is_Expired_Then_False_Is_Returned()
    {
        var confirmationCode = new ConfirmationCode { ExpiresIn = 10, IssueAt = DateTimeOffset.UtcNow.AddDays(-2) };
        _confirmationCodeStoreStub
            .Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(confirmationCode);

        var result = await _validateConfirmationCodeAction.Execute("code", "test", CancellationToken.None)
            ;

        Assert.False(result);
    }

    [Fact]
    public async Task When_Code_Is_Not_Expired_Then_True_Is_Returned()
    {
        var confirmationCode = new ConfirmationCode { ExpiresIn = 200, IssueAt = DateTimeOffset.UtcNow };
        _confirmationCodeStoreStub
            .Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(confirmationCode);

        var result = await _validateConfirmationCodeAction.Execute("code", "test", CancellationToken.None)
            ;

        Assert.True(result);
    }
}
