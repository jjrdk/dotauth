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

using SimpleAuth.Shared.Repositories;

namespace SimpleAuth.Tests.Api.Introspection.Actions;

using Moq;
using Parameters;
using Shared;
using Shared.Models;
using SimpleAuth;
using SimpleAuth.Api.Introspection;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using SimpleAuth.Shared.Responses;
using Xunit;

public sealed class PostIntrospectionActionFixture
{
    private readonly Mock<ITokenStore> _tokenStoreStub;
    private readonly PostIntrospectionAction _postIntrospectionAction;

    public PostIntrospectionActionFixture()
    {
        _tokenStoreStub = new Mock<ITokenStore>();
        _postIntrospectionAction = new PostIntrospectionAction(_tokenStoreStub.Object);
    }

    [Fact]
    public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
    {
        await Assert
            .ThrowsAsync<NullReferenceException>(
                () => _postIntrospectionAction.Execute(null, CancellationToken.None))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task WhenAccessTokenCannotBeExtractedThenTokenIsInactive()
    {
        var parameter = new IntrospectionParameter
        {
            ClientId = "test",
            ClientSecret = "test",
            TokenTypeHint = CoreConstants.StandardTokenTypeHintNames.AccessToken,
            Token = "token"
        };

        _tokenStoreStub.Setup(a => a.GetAccessToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null);

        var response = await _postIntrospectionAction.Execute(parameter, CancellationToken.None)
            .ConfigureAwait(false) as Option<OauthIntrospectionResponse>.Result;

        Assert.False(response.Item.Active);
    }

    [Fact]
    public async Task When_Passing_Expired_RefreshToken_Then_Result_Should_Be_Returned()
    {
        var parameter = new IntrospectionParameter
        {
            TokenTypeHint = CoreConstants.StandardTokenTypeHintNames.RefreshToken, Token = "token"
        };
        var grantedToken = new GrantedToken
        {
            Scope = "scope",
            ClientId = "client_id",
            IdTokenPayLoad = new JwtPayload
            {
                {OpenIdClaimTypes.Subject, "tester"}, {StandardClaimNames.Audiences, new[] {"audience"}}
            },
            CreateDateTime = DateTimeOffset.UtcNow.AddYears(-1),
            ExpiresIn = 0
        };
        _tokenStoreStub.Setup(a => a.GetRefreshToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => grantedToken);

        var response = await _postIntrospectionAction
            .Execute(parameter, CancellationToken.None)
            .ConfigureAwait(false) as Option<OauthIntrospectionResponse>.Result;

        var result = response.Item;
        Assert.False(result.Active);
    }

    [Fact]
    public async Task When_Passing_Active_RefreshToken_Then_Result_Should_Be_Returned()
    {
        const string clientId = "client_id";
        const string subject = "subject";
        const string audience = "audience";
        var audiences = new[] {audience};
        var parameter = new IntrospectionParameter
        {
            TokenTypeHint = CoreConstants.StandardTokenTypeHintNames.RefreshToken, Token = "token"
        };
        var grantedToken = new GrantedToken
        {
            Scope = "scope",
            ClientId = clientId,
            IdTokenPayLoad = new JwtPayload
            {
                {OpenIdClaimTypes.Subject, subject}, {StandardClaimNames.Audiences, audiences}
            },
            CreateDateTime = DateTimeOffset.UtcNow,
            ExpiresIn = 20000
        };

        _tokenStoreStub.Setup(a => a.GetRefreshToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(grantedToken));

        var response = await _postIntrospectionAction
            .Execute(parameter, CancellationToken.None)
            .ConfigureAwait(false) as Option<OauthIntrospectionResponse>.Result;

        var result = response.Item;
        Assert.True(result.Active);
        Assert.Equal(audience, result.Audience);
        Assert.Equal(subject, result.Subject);
    }

    [Fact]
    public async Task When_Passing_Expired_AccessToken_Then_Result_Should_Be_Returned()
    {
        var parameter = new IntrospectionParameter
        {
            TokenTypeHint = CoreConstants.StandardTokenTypeHintNames.AccessToken, Token = "token"
        };
        var grantedToken = new GrantedToken
        {
            Scope = "scope",
            ClientId = "client_id",
            IdTokenPayLoad = new JwtPayload
            {
                {OpenIdClaimTypes.Subject, "tester"}, {StandardClaimNames.Audiences, new[] {"audience"}}
            },
            CreateDateTime = DateTimeOffset.UtcNow.AddYears(-1),
            ExpiresIn = 0
        };
        _tokenStoreStub.Setup(a => a.GetAccessToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => grantedToken);

        var response = await _postIntrospectionAction
            .Execute(parameter, CancellationToken.None)
            .ConfigureAwait(false) as Option<OauthIntrospectionResponse>.Result;

        var result = response.Item;
        Assert.False(result.Active);
    }

    [Fact]
    public async Task When_Passing_Active_AccessToken_Then_Result_Should_Be_Returned()
    {
        const string clientId = "client_id";
        const string subject = "subject";
        const string audience = "audience";
        var audiences = new[] {audience};
        var parameter = new IntrospectionParameter
        {
            TokenTypeHint = CoreConstants.StandardTokenTypeHintNames.AccessToken, Token = "token"
        };
        var grantedToken = new GrantedToken
        {
            Scope = "scope",
            ClientId = clientId,
            IdTokenPayLoad = new JwtPayload
            {
                {OpenIdClaimTypes.Subject, subject}, {StandardClaimNames.Audiences, audiences}
            },
            CreateDateTime = DateTimeOffset.UtcNow,
            ExpiresIn = 20000
        };

        _tokenStoreStub.Setup(a => a.GetAccessToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(grantedToken));

        var response = await _postIntrospectionAction
            .Execute(parameter, CancellationToken.None)
            .ConfigureAwait(false) as Option<OauthIntrospectionResponse>.Result;

        var result = response.Item;
        Assert.True(result.Active);
        Assert.Equal(audience, result.Audience);
        Assert.Equal(subject, result.Subject);
    }
}