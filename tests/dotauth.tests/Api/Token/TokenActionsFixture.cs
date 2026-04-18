namespace DotAuth.Tests.Api.Token;

using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using DotAuth;
using DotAuth.Api.Token;
using DotAuth.Api.Token.Actions;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.Parameters;
using DotAuth.Repositories;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using DotAuth.Telemetry;
using DotAuth.Tests.Telemetry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;

public sealed class TokenActionsFixture
{
    private const string ClientId = "valid_client_id";
    private const string ClientSecret = "secret";
    private readonly TokenActions _tokenActions;

    public TokenActionsFixture()
    {
        var eventPublisher = Substitute.For<IEventPublisher>();
        const string scope = "valid_scope";
        var mock = Substitute.For<IClientStore>();
        mock.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(
                new Client
                {
                    JsonWebKeys =
                        "supersecretlongkey".CreateJwk(
                                JsonWebKeyUseNames.Sig,
                                KeyOperations.Sign,
                                KeyOperations.Verify)
                            .ToSet(),
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                    ClientId = ClientId,
                    Secrets =
                        [new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = ClientSecret }],
                    AllowedScopes = [scope],
                    ResponseTypes = [ResponseTypeNames.Token],
                    GrantTypes = [GrantTypes.ClientCredentials]
                });

        _tokenActions = new TokenActions(
            new RuntimeSettings(string.Empty),
            Substitute.For<IAuthorizationCodeStore>(),
            mock,
            Substitute.For<IScopeRepository>(),
            new InMemoryJwksRepository(),
            new InMemoryResourceOwnerRepository(string.Empty),
            [],
            eventPublisher,
            Substitute.For<ITokenStore>(),
            Substitute.For<IDeviceAuthorizationStore>(),
            Substitute.For<ILogger<TokenActions>>());
    }

    [Fact]
    public async Task When_Getting_Token_Via_ClientCredentials_GrantType_Then_GrantedToken_Is_Returned()
    {
        const string scope = "valid_scope";
        const string clientId = "valid_client_id";
        var parameter = new ClientCredentialsGrantTypeParameter { Scope = scope };

        var authenticationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientId}:{ClientSecret}".Base64Encode());
        var result = Assert.IsType<Option<GrantedToken>.Result>(await _tokenActions
            .GetTokenByClientCredentialsGrantType(
                parameter,
                authenticationHeader,
                null,
                "",
                CancellationToken.None)
        );

        Assert.Equal(clientId, result.Item.ClientId);
    }
}

public sealed class GetTokenByDeviceAuthorizationTypeActionTelemetryFixture
{
    [Fact]
    public async Task When_Device_Code_Is_Still_Pending_Then_Error_Telemetry_Is_Recorded()
    {
        var deviceAuthorizationStore = Substitute.For<IDeviceAuthorizationStore>();
        var tokenStore = Substitute.For<ITokenStore>();
        var clientStore = Substitute.For<IClientStore>();
        var eventPublisher = Substitute.For<IEventPublisher>();
        var action = new GetTokenByDeviceAuthorizationTypeAction(
            deviceAuthorizationStore,
            tokenStore,
            new InMemoryJwksRepository(),
            clientStore,
            eventPublisher,
            NullLogger.Instance);
        var request = new DeviceAuthorizationData
        {
            ClientId = "client",
            DeviceCode = "device-code",
            Interval = 5,
            Approved = false,
            Expires = DateTimeOffset.UtcNow.AddMinutes(5),
            LastPolled = DateTimeOffset.UtcNow.AddSeconds(-10),
            Scopes = ["openid"],
            Response = new DeviceAuthorizationResponse()
        };
        deviceAuthorizationStore.Get("client", "device-code", Arg.Any<CancellationToken>())
            .Returns(new Option<DeviceAuthorizationData>.Result(request));
        using var activityCollector = new ActivityCollector();
        using var metricCollector = new MetricCollector(DotAuthTelemetry.MetricNames.DeviceCodePolls);

        var result = Assert.IsType<Option<GrantedToken>.Error>(await action.Execute(
            "client",
            "device-code",
            "issuer",
            CancellationToken.None));

        Assert.Equal(ErrorCodes.AuthorizationPending, result.Details.Title);
        var activity = Assert.Single(activityCollector.Activities, candidate =>
            candidate.DisplayName == DotAuthTelemetry.ActivityNames.TokenDeviceCode);
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Contains(activity.Tags, tag =>
            tag.Key == DotAuthTelemetry.TagKeys.ErrorCode && Equals(tag.Value, ErrorCodes.AuthorizationPending));
        Assert.Contains(activity.Tags, tag =>
            tag.Key == DotAuthTelemetry.TagKeys.DeviceCodeStatus && Equals(tag.Value, "pending"));
        Assert.Contains(metricCollector.Measurements, measurement =>
            measurement.Name == DotAuthTelemetry.MetricNames.DeviceCodePolls && measurement.Value == 1);
    }
}

