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

namespace DotAuth.Tests.Api.Introspection.Actions;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.Introspection;
using DotAuth.Endpoints;
using DotAuth.Parameters;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Responses;
using DotAuth.Telemetry;
using DotAuth.Tests.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using Xunit;

public sealed class PostIntrospectionActionFixture
{
    private readonly ITokenStore _tokenStoreStub;
    private readonly PostIntrospectionAction _postIntrospectionAction;

    public PostIntrospectionActionFixture()
    {
        _tokenStoreStub = Substitute.For<ITokenStore>();
        _postIntrospectionAction = new PostIntrospectionAction(_tokenStoreStub);
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

        _tokenStoreStub.GetAccessToken(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(default(GrantedToken));

        var response = Assert.IsType<Option<OauthIntrospectionResponse>.Result>(await _postIntrospectionAction
            .Execute(parameter, CancellationToken.None)
        );

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
                { OpenIdClaimTypes.Subject, "tester" }, { StandardClaimNames.Audiences, new[] { "audience" } }
            },
            CreateDateTime = DateTimeOffset.UtcNow.AddYears(-1),
            ExpiresIn = 0
        };
        _tokenStoreStub.GetRefreshToken(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(grantedToken);

        var response = Assert.IsType<Option<OauthIntrospectionResponse>.Result>(await _postIntrospectionAction
            .Execute(parameter, CancellationToken.None)
        );

        var result = response.Item;
        Assert.False(result.Active);
    }

    [Fact]
    public async Task When_Passing_Active_RefreshToken_Then_Result_Should_Be_Returned()
    {
        const string clientId = "client_id";
        const string subject = "subject";
        const string audience = "audience";
        var audiences = new[] { audience };
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
                { OpenIdClaimTypes.Subject, subject }, { StandardClaimNames.Audiences, audiences }
            },
            CreateDateTime = DateTimeOffset.UtcNow,
            ExpiresIn = 20000
        };

        _tokenStoreStub.GetRefreshToken(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<GrantedToken?>(grantedToken));

        var response = Assert.IsType<Option<OauthIntrospectionResponse>.Result>(await _postIntrospectionAction
            .Execute(parameter, CancellationToken.None)
        );

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
                { OpenIdClaimTypes.Subject, "tester" }, { StandardClaimNames.Audiences, new[] { "audience" } }
            },
            CreateDateTime = DateTimeOffset.UtcNow.AddYears(-1),
            ExpiresIn = 0
        };
        _tokenStoreStub.GetAccessToken(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(grantedToken);

        var response = Assert.IsType<Option<OauthIntrospectionResponse>.Result>(await _postIntrospectionAction
            .Execute(parameter, CancellationToken.None)
        );

        var result = response.Item;
        Assert.False(result.Active);
    }

    [Fact]
    public async Task When_Passing_Active_AccessToken_Then_Result_Should_Be_Returned()
    {
        const string clientId = "client_id";
        const string subject = "subject";
        const string audience = "audience";
        var audiences = new[] { audience };
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
                { OpenIdClaimTypes.Subject, subject }, { StandardClaimNames.Audiences, audiences }
            },
            CreateDateTime = DateTimeOffset.UtcNow,
            ExpiresIn = 20000
        };

        _tokenStoreStub.GetAccessToken(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<GrantedToken?>(grantedToken));

        var response = Assert.IsType<Option<OauthIntrospectionResponse>.Result>(await _postIntrospectionAction
            .Execute(parameter, CancellationToken.None)
        );

        var result = response.Item;
        Assert.True(result.Active);
        Assert.Equal(audience, result.Audience);
        Assert.Equal(subject, result.Subject);
    }
}

public sealed class IntrospectionEndpointTelemetryFixture
{
    [Fact]
    public async Task When_Posting_Introspection_Without_Token_Then_Error_Telemetry_Is_Recorded()
    {
        var requestThrottle = Substitute.For<IRequestThrottle>();
        requestThrottle.Allow(Arg.Any<HttpRequest>()).Returns(true);
        var tokenStore = Substitute.For<ITokenStore>();
        var httpContext = CreateFormHttpContext(
            "/introspect",
            new Dictionary<string, string> { ["token_type_hint"] = "access_token" });
        using var activityCollector = new ActivityCollector();
        using var metricCollector = new MetricCollector(
            DotAuthTelemetry.MetricNames.IntrospectionRequests,
            DotAuthTelemetry.MetricNames.IntrospectionInactive);

        var result = await IntrospectionEndpointHandlers.PostIntrospection(
            httpContext,
            requestThrottle,
            tokenStore,
            CancellationToken.None);

        Assert.NotNull(result);
        var activity = Assert.Single(activityCollector.Activities, candidate =>
            candidate.DisplayName == DotAuthTelemetry.ActivityNames.IntrospectionRequest);
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Contains(activity.Tags, tag =>
            tag.Key == DotAuthTelemetry.TagKeys.ErrorCode && Equals(tag.Value, ErrorCodes.InvalidRequest));
        Assert.Contains(metricCollector.Measurements, measurement =>
            measurement.Name == DotAuthTelemetry.MetricNames.IntrospectionRequests && measurement.Value == 1);
        Assert.Contains(metricCollector.Measurements, measurement =>
            measurement.Name == DotAuthTelemetry.MetricNames.IntrospectionInactive && measurement.Value == 1);
    }

    private static DefaultHttpContext CreateFormHttpContext(
        string path,
        IReadOnlyDictionary<string, string> formValues)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost", 8001);
        httpContext.Request.Path = path;
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.ContentType = "application/x-www-form-urlencoded";
        httpContext.Features.Set<IFormFeature>(
            new FormFeature(
                new FormCollection(
                    formValues.ToDictionary(
                        kvp => kvp.Key,
                        kvp => new StringValues(kvp.Value)))));
        return httpContext;
    }
}

