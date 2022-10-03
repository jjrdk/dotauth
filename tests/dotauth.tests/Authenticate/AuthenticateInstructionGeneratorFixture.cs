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

namespace DotAuth.Tests.Authenticate;

using System.Net.Http.Headers;
using DotAuth.Extensions;
using DotAuth.Shared;
using Xunit;

public sealed class AuthenticateInstructionGeneratorFixture
{
    [Fact]
    public void When_Passing_No_Parameter_Then_Empty_Result_Is_Returned()
    {
        var header = (AuthenticationHeaderValue) null;
        var result = header.GetAuthenticateInstruction(null);

        Assert.True(string.IsNullOrWhiteSpace(result.ClientIdFromAuthorizationHeader));
        Assert.True(string.IsNullOrWhiteSpace(result.ClientSecretFromAuthorizationHeader));
    }

    [Fact]
    public void When_Passing_Empty_AuthenticationHeaderParameter_Then_Empty_Result_Is_Returned()
    {
        var authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", string.Empty);

        var result = authenticationHeaderValue.GetAuthenticateInstruction(null);

        Assert.True(string.IsNullOrWhiteSpace(result.ClientIdFromAuthorizationHeader));
        Assert.True(string.IsNullOrWhiteSpace(result.ClientSecretFromAuthorizationHeader));
    }

    [Fact]
    public void When_Passing_Not_Valid_Parameter_Then_Empty_Result_Is_Returned()
    {
        var parameter = "parameter";
        var encodedParameter = parameter.Base64Encode();
        var authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", encodedParameter);

        var result = authenticationHeaderValue.GetAuthenticateInstruction(null);

        Assert.True(string.IsNullOrWhiteSpace(result.ClientIdFromAuthorizationHeader));
        Assert.True(string.IsNullOrWhiteSpace(result.ClientSecretFromAuthorizationHeader));
    }

    [Fact]
    public void When_Passing_Valid_Parameter_Then_Valid_AuthenticateInstruction_Is_Returned()
    {
        const string clientId = "clientId";
        const string clientSecret = "clientSecret";
        var parameter = $"{clientId}:{clientSecret}";
        var encodedParameter = parameter.Base64Encode();
        var authenticationHeaderValue = new AuthenticationHeaderValue("Basic", encodedParameter);

        var result = authenticationHeaderValue.GetAuthenticateInstruction(null);

        Assert.Equal(clientId, result.ClientIdFromAuthorizationHeader);
        Assert.Equal(clientSecret, result.ClientSecretFromAuthorizationHeader);
    }
}